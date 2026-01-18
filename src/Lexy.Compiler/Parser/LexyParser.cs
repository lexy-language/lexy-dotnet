using System;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.RunTime;
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
        this.sourceCodeDocument = Assert.NotNull(sourceCodeDocument, nameof(sourceCodeDocument));
        this.baseLogger = Assert.NotNull(baseLogger, nameof(baseLogger));
        this.tokenizer = Assert.NotNull(tokenizer, nameof(tokenizer));
        this.fileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        this.expressionFactory = Assert.NotNull(expressionFactory, nameof(expressionFactory));
        this.libraries = Assert.NotNull(libraries, nameof(libraries));
    }

    public async Task<ParserResult> ParseFile(string fileName, ParseOptions options)
    {
        baseLogger.LogInformation("Parse file: {FileName}", fileName);

        var code = await fileSystem.ReadAllLines(fileName);
        return await Parse(code, fileName, options);
    }

    public async Task<ParserResult> Parse(string[] code, string fullFileName, ParseOptions options)
    {
        Assert.NotNull(code, nameof(code));

        var parserLogger = new ParserLogger(baseLogger);
        var context = new ParserContext(parserLogger, fileSystem, libraries, options);

        context.AddFileIncluded(fullFileName);
        context.SetFileLineFilter(fullFileName);

        await ParseDocument(code, fullFileName, context);

        parserLogger.LogNodes(context.Nodes);

        var dependencies = SortByDependencyAndCheckCircularDependencies(context);

        if (!dependencies.HasCircularReferences)
        {
            context.RootNode.SortByDependency(dependencies.SortedNodes);
            ValidateNodesTree(context);
        }

        if (!context.Options.SuppressException)
        {
            parserLogger.AssertNoErrors();
        }

        return new ParserResult(context.RootNode, context.Nodes, context.Logger, dependencies);
    }

    private async Task ParseDocument(string[] code, string fullFileName, IParserContext context)
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

        await LoadIncludedFiles(fullFileName, context);
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

        return tokens.IsSuccess;
    }

    private async Task LoadIncludedFiles(string parentFullFileName, IParserContext context)
    {
        var includes = context.RootNode.GetDueIncludes();
        foreach (var include in includes)
        {
            await IncludeFiles(parentFullFileName, include, context);
        }
    }

    private async Task IncludeFiles(string parentFullFileName, Include include, IParserContext context)
    {
        var fileName = await include.Process(parentFullFileName, context);
        if (fileName == null) return;

        if (context.IsFileIncluded(fileName)) return;

        context.Logger.LogInfo("Parse file: " + fileName);

        var code = await fileSystem.ReadAllLines(fileName);

        context.AddFileIncluded(fileName);

        await ParseDocument(code, fileName, context);
    }

    private void ValidateNodesTree(IParserContext context)
    {
        var visitor = new TrackLoggingCurrentNodeVisitor(context.Logger);
        var validationContext = new ValidationContext(context.Logger, context.Nodes, visitor, context.Libraries);
        SetParents(context);
        context.RootNode.ValidateTree(validationContext);
    }

    private static void SetParents(IParserContext context)
    {
        NodesWalker.Walk(context.RootNode, (node, parent) =>
        {
            if (node is not INodeWithParent nodeWithParent)
            {
                throw new InvalidOperationException("Each node should implement INodeWithParent");
            }

            nodeWithParent.SetParent(parent);
        }, null);
    }

    private Dependencies SortByDependencyAndCheckCircularDependencies(IParserContext context)
    {
        var dependencies = DependencyGraphFactory.Create(context.Nodes);
        if (!dependencies.HasCircularReferences) return dependencies;

        foreach (var circularReference in dependencies.CircularReferences)
        {
            context.Logger.SetCurrentNode(circularReference.Value);
            context.Logger.Fail(circularReference.Value.Reference,
                $"Circular reference detected in: '{circularReference.Key}'");
        }
        return dependencies;
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
