using System;
using System.IO;
using System.Linq;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Parser;

public class LexyParser : ILexyParser
{
    private readonly ILogger baseLogger;
    private readonly ITokenizer tokenizer;
    private readonly IExpressionFactory expressionFactory;
    private readonly ISourceCodeDocument sourceCodeDocument;

    public LexyParser(ISourceCodeDocument sourceCodeDocument, ILogger<LexyParser> baseLogger, ITokenizer tokenizer, IExpressionFactory expressionFactory)
    {
        this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
        this.baseLogger = baseLogger ?? throw new ArgumentNullException(nameof(baseLogger));
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        this.expressionFactory = expressionFactory ?? throw new ArgumentNullException(nameof(expressionFactory));
    }

    public ParserResult ParseFile(string fileName, bool throwException = true)
    {
        baseLogger.LogInformation("Parse file: {FileName}", fileName);

        var code = File.ReadAllLines(fileName);
        return Parse(code, fileName, throwException);
    }

    public ParserResult Parse(string[] code, string fullFileName, bool throwException = true)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        var parserLogger = new ParserLogger(baseLogger);
        var context = new ParserContext(parserLogger);

        context.AddFileIncluded(fullFileName);

        ParseDocument(code, fullFileName, context);

        parserLogger.LogNodes(context.Nodes);

        ValidateNodesTree(context);
        DetectCircularDependencies(context);

        if (throwException) parserLogger.AssertNoErrors();

        return new ParserResult(context.Nodes, context.Logger);
    }

    private void ParseDocument(string[] code, string fullFileName, IParserContext context)
    {
        sourceCodeDocument.SetCode(code, Path.GetFileName(fullFileName));

        var currentIndent = 0;
        var nodePerIndent = new ParsableNodeArray(context.RootNode);

        while (sourceCodeDocument.HasMoreLines())
        {
            if (!ProcessLine(context.Logger))
            {
                currentIndent = sourceCodeDocument.CurrentLine?.Indent(context.Logger) ?? currentIndent;
                continue;
            }

            var line = sourceCodeDocument.CurrentLine;
            if (line.IsEmpty()) continue;

            var indentResult = line.Indent(context.Logger);
            if (!indentResult.HasValue) continue;

            var indent = indentResult.Value;
            if (indent > currentIndent)
            {
                context.Logger.Fail(line.LineStartReference(), $"Invalid indent: {indent}");
                continue;
            }

            var node = nodePerIndent.Get(indent);
            node = ParseLine(node, context);

            currentIndent = indent + 1;

            nodePerIndent.Set(currentIndent, node);
        }

        Reset(context);

        LoadIncludedFiles(fullFileName, context);
    }

    private bool ProcessLine(IParserLogger logger)
    {
        var line = sourceCodeDocument.NextLine();
        logger.Log(line.LineStartReference(), $"'{line.Content}'");

        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            logger.Fail(tokens.Reference, tokens.ErrorMessage);
            return false;
        }

        var tokenNames = string.Join(" ", sourceCodeDocument.CurrentLine.Tokens.Select(token =>
            $"{token.GetType().Name}({token.Value})").ToArray());

        logger.Log(line.LineStartReference(), "  Tokens: " + tokenNames);

        return tokens.IsSuccess;
    }

    private void LoadIncludedFiles(string parentFullFileName, IParserContext context)
    {
        var includes = context.RootNode.GetDueIncludes();
        foreach (var include in includes) IncludeFiles(parentFullFileName, include, context);
    }

    private void IncludeFiles(string parentFullFileName, Include include, IParserContext context)
    {
        var fileName = include.Process(parentFullFileName, context);
        if (fileName == null) return;

        if (context.IsFileIncluded(fileName)) return;

        context.Logger.LogInfo("Parse file: " + fileName);

        var code = File.ReadAllLines(fileName);

        context.AddFileIncluded(fileName);

        ParseDocument(code, fileName, context);
    }

    private void ValidateNodesTree(IParserContext context)
    {
        var validationContext = new ValidationContext(context.Logger, context.Nodes);
        context.RootNode.ValidateTree(validationContext);
    }

    private void DetectCircularDependencies(IParserContext context)
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

    private void Reset(IParserContext context)
    {
        sourceCodeDocument.Reset();
        context.Logger.ResetCurrentNode();
    }

    private IParsableNode ParseLine(IParsableNode currentNode, IParserContext context)
    {
        var parseLineContext = new ParseLineContext(sourceCodeDocument.CurrentLine, context.Logger, expressionFactory);
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