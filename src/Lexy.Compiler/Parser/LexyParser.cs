using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
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

    public LexyParser(ILogger<LexyParser> baseLogger, ITokenizer tokenizer, IFileSystem fileSystem, ILibraries libraries)
    {
        this.baseLogger = Assert.NotNull(baseLogger, nameof(baseLogger));
        this.tokenizer = Assert.NotNull(tokenizer, nameof(tokenizer));
        this.fileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        this.libraries = Assert.NotNull(libraries, nameof(libraries));
    }

    public async Task<ParserResult> ParseCode(string fileName, string[] content, ParseOptions options)
    {
        Assert.NotNull(fileName, nameof(fileName));
        Assert.NotNull(content, nameof(content));
        Assert.NotNull(options, nameof(options));

        baseLogger.LogInformation("Parse code: {FileName}", fileName);

        var project = new Project(fileSystem);
        var document = new StringSourceCodeDocument(project.File(fileName), content);
        return await ParseDocuments(project, new[] { document }, options);
    }

    public async Task<ParserResult> ParseFile(IFile file, ParseOptions options)
    {
        Assert.NotNull(file, nameof(file));
        Assert.NotNull(options, nameof(options));

        baseLogger.LogInformation("Parse file: {FileName}", file.Name);

        using var document = await fileSystem.CreateFileSourceDocument(file);
        return await ParseDocuments(file.Project, new[] { document }, options);
    }

    public async Task<ParserResult> ParseFiles(IEnumerable<string> fileNames, ParseOptions options)
    {
        Assert.NotNull(fileNames, nameof(fileNames));
        Assert.NotNull(options, nameof(options));

        baseLogger.LogInformation("Parse files: {FileNames}", string.Join(", ", fileNames));

        var project = new Project(fileSystem);
        var files = fileNames.Select(fileName => project.File(fileName)).ToArray();
        using var documents = await fileSystem.CreateFileSourceDocuments(files);

        return await ParseDocuments(project, documents.Documents, options);
    }

    public async Task<ParserResult> ParseDocuments(IProject project, IEnumerable<ISourceCodeDocument> sourceCodeDocuments, ParseOptions options)
    {
        Assert.NotNull(sourceCodeDocuments, nameof(sourceCodeDocuments));

        var parserLogger = new ParserLogger(baseLogger);
        var context = new ParserContext(project, parserLogger, fileSystem, libraries, options);

        foreach (var sourceCodeDocument in sourceCodeDocuments)
        {
            context.AddFileIncluded(sourceCodeDocument.File);
            context.SetFileLineFilter(sourceCodeDocument.File);

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
        var symbols = context.Symbols.Document(sourceCodeDocument.File);

        while (sourceCodeDocument.HasMoreLines())
        {
            var line = sourceCodeDocument.NextLine();
            symbols.Add(line);

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

        await LoadIncludedFiles(sourceCodeDocument.File, context);
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

    private async Task LoadIncludedFiles(IFile parentFile, IParserContext context)
    {
        var includes = context.RootNode.GetDueIncludes();
        foreach (var include in includes)
        {
            await IncludeFiles(parentFile, include, context);
        }
    }

    private async Task IncludeFiles(IFile parentFile, Include include, IParserContext context)
    {
        var file = await include.Process(parentFile, context);
        if (file == null) return;

        if (context.IsFileIncluded(file)) return;

        context.Logger.LogInfo("Parse file: " + file.Name);

        context.AddFileIncluded(file);

        var document = await fileSystem.CreateFileSourceDocument(file);
        await ParseDocument(document, context);
    }

    private static void ValidateNodesTree(IParserContext context)
    {
        var visitor = new TrackLoggingCurrentNodeVisitor(context.Logger);
        var validationContext = new ValidationContext(context.Logger, context.Nodes, visitor, context.Libraries, context.Symbols);
        context.RootNode.ValidateTree(validationContext);
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
        IDocumentSymbols documentSymbols, ParsableNodeIndex nodesPerIndent, int indent)
    {
        if (currentNode == null)
        {
            throw new InvalidOperationException($"Current node can't be null. Line: {line}");
        }

        var parseLineContext = new ParseLineContext(line, context.Logger, documentSymbols);
        currentNode.ExpandArea(line.EndPosition);

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
