using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Documents;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
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

    public LexyParser(ILogger<LexyParser> baseLogger, ITokenizer tokenizer, IFileSystem fileSystem, IExpressionFactory expressionFactory, ILibraries libraries)
    {
        this.baseLogger = Assert.NotNull(baseLogger, nameof(baseLogger));
        this.tokenizer = Assert.NotNull(tokenizer, nameof(tokenizer));
        this.fileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        this.expressionFactory = Assert.NotNull(expressionFactory, nameof(expressionFactory));
        this.libraries = Assert.NotNull(libraries, nameof(libraries));
    }

    public async Task<ParserResult> ParseCode(string fileName, string[] content, ParseOptions options)
    {
        Assert.NotNull(fileName, nameof(fileName));
        Assert.NotNull(content, nameof(content));

        baseLogger.LogInformation("Parse code: {FileName}", fileName);

        var document = new StringSourceCodeDocument(content, fileName);
        return await ParseDocuments(new[] { document }, options);
    }

    public async Task<ParserResult> ParseFile(string fileName, ParseOptions options)
    {
        baseLogger.LogInformation("Parse file: {FileName}", fileName);

        var fullPath = fileSystem.GetFullPath(fileName);
        using var document = new FileSourceDocument(fullPath);
        return await ParseDocuments(new[] { document }, options);
    }

    public async Task<ParserResult> ParseFiles(IEnumerable<string> fileNames, ParseOptions options)
    {
        baseLogger.LogInformation("Parse files: {FileNames}", string.Join(", ", fileNames));

        using var documents = FileDocuments.Create(fileSystem, fileNames);

        return await ParseDocuments(documents.Documents, options);
    }

    public async Task<ParserResult> ParseDocuments(IEnumerable<ISourceCodeDocument> sourceCodeDocuments, ParseOptions options)
    {
        Assert.NotNull(sourceCodeDocuments, nameof(sourceCodeDocuments));

        var parserLogger = new ParserLogger(baseLogger);
        var context = new ParserContext(parserLogger, fileSystem, libraries, options);

        foreach (var sourceCodeDocument in sourceCodeDocuments)
        {
            context.AddFileIncluded(sourceCodeDocument.FullFileName);
            context.SetFileLineFilter(sourceCodeDocument.FullFileName);

            await ParseDocument(sourceCodeDocument, context);
        }
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

        return new ParserResult(context.RootNode, context.Nodes, context.Logger, dependencies, context.Symbols);
    }

    private async Task ParseDocument(ISourceCodeDocument sourceCodeDocument, IParserContext context)
    {
        var currentIndent = 0;
        var nodesPerIndent = new ParsableNodeIndex(context.RootNode);
        var symbols = context.Symbols.Document(sourceCodeDocument.FullFileName);

        while (sourceCodeDocument.HasMoreLines())
        {
            var line = sourceCodeDocument.NextLine();
            if (!TokenizeLine(line, context))
            {
                currentIndent = line?.Indent(context.Logger) ?? currentIndent;
                continue;
            }

            if (!GetIndent(context, line, out var indent)) continue;

            if (indent > currentIndent)
            {
                context.Logger.Fail(line.Tokens.AllReference(), $"Invalid indent: {indent}");
                continue;
            }

            var node = nodesPerIndent.GetCurrentOrDescend(indent);
            var parsedNode = ParseLine(line, node, context, symbols, nodesPerIndent, indent);

            currentIndent = indent + 1;

            nodesPerIndent.Set(currentIndent, parsedNode);
        }

        Reset(context);

        await LoadIncludedFiles(sourceCodeDocument.FullFileName, context);
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

    private bool TokenizeLine(Line line, IParserContext context)
    {
        var reference = line.LineReference(0);
        if (!context.LineFilter.UseLine(line.Content))
        {
            context.Logger.Log(reference, $"Skip line by filter: '{line.Content}'");
            return false;
        }

        context.Logger.Log(reference, $"'{line.Content}'");

        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            context.Logger.Fail(tokens.Reference, tokens.ErrorMessage);
            return false;
        }
        var allTokensReference = line.Tokens.AllReference();

        var tokenNames = string.Join(" ", line.Tokens.
            Select(token => $"{token.GetType().Name}({token.Value})").ToArray());

        context.Logger.Log(allTokensReference, "  Tokens: " + tokenNames);

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

        context.AddFileIncluded(fileName);

        var document = new FileSourceDocument(fileName);
        await ParseDocument(document, context);
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
        context.Logger.ResetCurrentNode();
    }

    private IParsableNode ParseLine(Line line, IParsableNode currentNode, IParserContext context,
        DocumentSymbols documentSymbols, ParsableNodeIndex nodesPerIndent, int indent)
    {
        if (currentNode == null)
        {
            throw new InvalidOperationException($"Current node can't be null. Line: {line}");
        }

        var parseLineContext = new ParseLineContext(line, context.Logger, documentSymbols, expressionFactory);
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
