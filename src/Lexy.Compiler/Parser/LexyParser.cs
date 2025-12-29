using System;
using System.Linq;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Parser;

public class LexyParser : ILexyParser
{
    private readonly ILogger baseLogger;
    private readonly ITokenizer tokenizer;
    private readonly IFileSystem fileSystem;
    private readonly ILibraries libraries;
    private readonly IExpressionFactory expressionFactory;
    private readonly ISourceCodeDocument sourceCodeDocument;

    public LexyParser(ISourceCodeDocument sourceCodeDocument, ILogger<LexyParser> baseLogger, ITokenizer tokenizer, IFileSystem fileSystem, IExpressionFactory expressionFactory, ILibraries libraries)
    {
        this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
        this.baseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.expressionFactory = expressionFactory ?? throw new ArgumentNullException(nameof(expressionFactory));
        this.libraries = libraries ?? throw new ArgumentNullException(nameof(libraries));
    }

    public ParserResult ParseFile(string fileName, ParseOptions options)
    {
        baseLogger.LogInformation("Parse file: {FileName}", fileName);

        var code = fileSystem.ReadAllLines(fileName);
        return Parse(code, fileName, options);
    }

    public ParserResult Parse(string[] code, string fullFileName, ParseOptions options)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        var parserLogger = new ParserLogger(baseLogger);
        var context = new ParserContext(parserLogger, fileSystem, libraries, options);

        context.AddFileIncluded(fullFileName);
        context.SetFileLineFilter(fullFileName);

        ParseDocument(code, fullFileName, context);

        parserLogger.LogNodes(context.Nodes);

        var dependencyGraph = SortByDependencyAndCheckCircularDependencies(context);
        if (dependencyGraph != null)
        {
            context.RootNode.SortByDependency(dependencyGraph.SortedNodes);
            ValidateNodesTree(context);
        }

        if (!context.Options.SuppressException)
        {
            parserLogger.AssertNoErrors();
        }

        return new ParserResult(context.Nodes, context.Logger);
    }

    private void ParseDocument(string[] code, string fullFileName, IParserContext context)
    {
        sourceCodeDocument.SetCode(code, fileSystem.GetFileName(fullFileName));

        var currentIndent = 0;
        var nodesPerIndent = new ParsableNodeIndex(context.RootNode);

        while (sourceCodeDocument.HasMoreLines())
        {
            if (!TokenizeLine(context))
            {
                currentIndent = sourceCodeDocument.CurrentLine?.Indent(context.Logger) ?? currentIndent;
                continue;
            }

            var line = sourceCodeDocument.CurrentLine;
            if (!GetIndent(context, line, out var indent)) continue;

            if (indent > currentIndent)
            {
                context.Logger.Fail(line.LineStartReference(), $"Invalid indent: {indent}");
                continue;
            }

            var node = nodesPerIndent.GetCurrentOrDescend(indent);
            var parsedNode = ParseLine(node, context, nodesPerIndent, indent);

            currentIndent = indent + 1;

            nodesPerIndent.Set(currentIndent, parsedNode);
        }

        Reset(context);

        LoadIncludedFiles(fullFileName, context);
    }

    private bool GetIndent(IParserContext context, Line line, out int indent)
    {
        indent = default;

        if (line.IsEmpty()) return false;

        var indentResult = line.Indent(context.Logger);
        if (!indentResult.HasValue) return false;

        indent = indentResult.Value;

        return true;
    }

    private bool TokenizeLine(IParserContext context)
    {
        var line = sourceCodeDocument.NextLine();
        if (!context.LineFilter.UseLine(line.Content)) {
            context.Logger.Log(line.LineStartReference(), @$"Skip line by filter: '{line.Content}'");
            return false;
        }

        context.Logger.Log(line.LineStartReference(), $"'{line.Content}'");

        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            context.Logger.Fail(tokens.Reference, tokens.ErrorMessage);
            return false;
        }

        var tokenNames = string.Join(" ", sourceCodeDocument.CurrentLine.Tokens.Select(token =>
            $"{token.GetType().Name}({token.Value})").ToArray());

        context.Logger.Log(line.LineStartReference(), "  Tokens: " + tokenNames);

        return tokens.IsSuccess;
    }

    private void LoadIncludedFiles(string parentFullFileName, IParserContext context)
    {
        var includes = context.RootNode.GetDueIncludes();
        foreach (var include in includes)
        {
            IncludeFiles(parentFullFileName, include, context);
        }
    }

    private void IncludeFiles(string parentFullFileName, Include include, IParserContext context)
    {
        var fileName = include.Process(parentFullFileName, context);
        if (fileName == null) return;

        if (context.IsFileIncluded(fileName)) return;

        context.Logger.LogInfo("Parse file: " + fileName);

        var code = fileSystem.ReadAllLines(fileName);

        context.AddFileIncluded(fileName);

        ParseDocument(code, fileName, context);
    }

    private void ValidateNodesTree(IParserContext context)
    {
        var visitor = new TrackLoggingCurrentNodeVisitor(context.Logger);
        var validationContext = new ValidationContext(context.Logger, context.Nodes, visitor, context.Libraries);
        context.RootNode.ValidateTree(validationContext);
    }

    private Dependencies SortByDependencyAndCheckCircularDependencies(IParserContext context)
    {
        var dependencies = DependencyGraphFactory.Create(context.Nodes);
        if (!dependencies.HasCircularReferences) return dependencies;

        foreach (var circularReference in dependencies.CircularReferences)
        {
            context.Logger.SetCurrentNode(circularReference);
            context.Logger.Fail(circularReference.Reference,
                $"Circular reference detected in: '{circularReference.NodeName}'");
        }

        return null;
    }

    private void Reset(IParserContext context)
    {
        sourceCodeDocument.Reset();
        context.Logger.ResetCurrentNode();
    }

    private IParsableNode ParseLine(IParsableNode currentNode, IParserContext context, ParsableNodeIndex nodesPerIndent, int indent)
    {
        if (currentNode == null)
        {
            throw new InvalidOperationException($"Current node can't be null. Line: {sourceCodeDocument.CurrentLine}");
        }

        var parseLineContext = new ParseLineContext(sourceCodeDocument.CurrentLine, context.Logger, expressionFactory);
        var node = currentNode.Parse(parseLineContext);
        if (node == null)
        {
            throw new InvalidOperationException($"({currentNode}) Parse should return child node or itself.");
        }

        if (node is IComponentNode componentNode)
        {
            context.Logger.SetCurrentNode(componentNode);
        }
        else
        {
            var parentComponentNode = nodesPerIndent.GetParentComponent(indent);
            context.Logger.SetCurrentNode(parentComponentNode);
        }

        return node;
    }
}