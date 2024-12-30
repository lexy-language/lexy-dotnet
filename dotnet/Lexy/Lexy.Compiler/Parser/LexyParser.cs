using System;
using System.IO;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class LexyParser : ILexyParser
{
    private readonly IParserContext context;
    private readonly IParserLogger logger;
    private readonly ISourceCodeDocument sourceCodeDocument;

    public LexyParser(IParserContext parserContext, ISourceCodeDocument sourceCodeDocument, IParserLogger logger)
    {
        context = parserContext ?? throw new ArgumentNullException(nameof(parserContext));
        this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ParserResult ParseFile(string fileName, bool throwException = true)
    {
        logger.LogInfo("Parse file: " + fileName);

        var code = File.ReadAllLines(fileName);
        return Parse(code, fileName, throwException);
    }

    public ParserResult Parse(string[] code, string fullFileName, bool throwException = true)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        context.AddFileIncluded(fullFileName);

        ParseDocument(code, fullFileName);

        logger.LogNodes(context.Nodes);

        ValidateNodesTree();
        DetectCircularDependencies();

        if (throwException) logger.AssertNoErrors();

        return new ParserResult(context.Nodes);
    }

    private void ParseDocument(string[] code, string fullFileName)
    {
        sourceCodeDocument.SetCode(code, Path.GetFileName(fullFileName));

        var currentIndent = 0;
        var nodePerIndent = new ParsableNodeArray(context.RootNode);

        while (sourceCodeDocument.HasMoreLines())
        {
            if (!context.ProcessLine())
            {
                currentIndent = sourceCodeDocument.CurrentLine?.Indent(context) ?? currentIndent;
                continue;
            }

            var line = sourceCodeDocument.CurrentLine;
            if (line.IsEmpty()) continue;

            var indentResult = line.Indent(context);
            if (!indentResult.HasValue) continue;

            var indent = indentResult.Value;
            if (indent > currentIndent)
            {
                context.Logger.Fail(line.LineStartReference(), $"Invalid indent: {indent}");
                continue;
            }

            var node = nodePerIndent.Get(indent);
            node = ParseLine(node);

            currentIndent = indent + 1;

            nodePerIndent.Set(currentIndent, node);
        }

        Reset();

        LoadIncludedFiles(fullFileName);
    }

    private void LoadIncludedFiles(string parentFullFileName)
    {
        var includes = context.RootNode.GetDueIncludes();
        foreach (var include in includes) IncludeFiles(parentFullFileName, include);
    }

    private void IncludeFiles(string parentFullFileName, Include include)
    {
        var fileName = include.Process(parentFullFileName, context);
        if (fileName == null) return;

        if (context.IsFileIncluded(fileName)) return;

        logger.LogInfo("Parse file: " + fileName);

        var code = File.ReadAllLines(fileName);

        context.AddFileIncluded(fileName);

        ParseDocument(code, fileName);
    }

    private void ValidateNodesTree()
    {
        var validationContext = new ValidationContext(context);
        context.RootNode.ValidateTree(validationContext);
    }

    private void DetectCircularDependencies()
    {
        var dependencies = DependencyGraphFactory.Create(context.Nodes);
        if (!dependencies.HasCircularReferences) return;

        foreach (var circularReference in dependencies.CircularReferences)
        {
            context.Logger.SetCurrentNode(circularReference);
            context.Logger.Fail(circularReference.Reference,
                $"Circular reference detected in: '{circularReference.NodeName}'");
        }
    }

    private void Reset()
    {
        sourceCodeDocument.Reset();
        logger.Reset();
    }

    private IParsableNode ParseLine(IParsableNode currentNode)
    {
        var parseLineContext = new ParseLineContext(context.CurrentLine, context.Logger);
        var node = currentNode.Parse(parseLineContext);
        if (node == null)
        {
            throw new InvalidOperationException($"({currentNode}) Parse should return child node or itself.");
        }

        if (node is IRootNode rootNode)
        {
            context.Logger.SetCurrentNode(rootNode);
        }

        return node;
    }
}

public interface IParseLineContext
{
    Line Line { get; }
    IParserLogger Logger { get; }

    TokenValidator ValidateTokens<T>();
    TokenValidator ValidateTokens(string name);
}

public class ParseLineContext : IParseLineContext
{
    public Line Line { get; }
    public IParserLogger Logger { get; }

    public ParseLineContext(Line line, IParserLogger logger)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TokenValidator ValidateTokens<T>()
    {
        return new TokenValidator(typeof(T).Name, Line, Logger);
    }

    public TokenValidator ValidateTokens(string name)
    {
        return new TokenValidator(name, Line, Logger);
    }
}