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
        var nodes = new ParsableNodeArray(context.RootNode);

        while (sourceCodeDocument.HasMoreLines())
        {
            if (!context.ProcessLine())
            {
                currentIndent = sourceCodeDocument.CurrentLine?.Indent(context) ?? currentIndent;
                continue;
            }

            var line = sourceCodeDocument.CurrentLine;
            if (line.IsComment() || line.IsEmpty()) continue;

            var indentResult = line.Indent(context);
            if (!indentResult.HasValue) continue;

            var indent = indentResult.Value;
            if (indent > currentIndent)
            {
                context.Logger.Fail(context.LineStartReference(), $"Invalid indent: {indent}");
                continue;
            }

            var node = nodes.Get(indent);
            node = ParseLine(node);

            currentIndent = indent + 1;

            nodes.Set(currentIndent, node);
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
        var node = currentNode.Parse(context);
        if (node == null)
            throw new InvalidOperationException(
                $"({currentNode}) Parse should return child node or itself.");
        return node;
    }
}