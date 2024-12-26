using System;
using System.IO;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class LexyParser : ILexyParser
    {
        private readonly IParserContext context;
        private readonly ISourceCodeDocument sourceCodeDocument;
        private readonly IParserLogger logger;

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

            var sourceCodeNode = new SourceCodeNode();
            context.AddFileIncluded(fullFileName);

            ParseDocument(sourceCodeNode, code, fullFileName);

            logger.LogNodes(context.Nodes);

            ValidateNodesTree(sourceCodeNode);

            if (throwException)
            {
                logger.AssertNoErrors();
            }

            return new ParserResult(context.Nodes);
        }

        private void ParseDocument(SourceCodeNode sourceCodeNode, string[] code, string fullFileName)
        {
            sourceCodeDocument.SetCode(code, Path.GetFileName(fullFileName));

            var currentIndent = 0;
            var nodes = new ParsableNodeArray(sourceCodeNode);

            while (sourceCodeDocument.HasMoreLines())
            {
                if (!context.ProcessLine())
                {
                    currentIndent = sourceCodeDocument.CurrentLine?.Indent(context) ?? currentIndent;
                    continue;
                }

                var line = sourceCodeDocument.CurrentLine;
                if (line.IsComment() || line.IsEmpty())
                {
                    continue;
                }

                var indentResult = line.Indent(context);
                if (!indentResult.HasValue)
                {
                    continue;
                }

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

            Finalize();

            LoadIncludedFiles(fullFileName, sourceCodeNode);
        }

        private void LoadIncludedFiles(string parentFullFileName, SourceCodeNode sourceCodeNode)
        {
            var includes = sourceCodeNode.GetDueIncludes();
            foreach (var include in includes)
            {
                IncludeFiles(parentFullFileName, sourceCodeNode, include);
            }
        }

        private void IncludeFiles(string parentFullFileName, SourceCodeNode sourceCodeNode, Include include)
        {
            var fileName = include.Process(parentFullFileName, context);
            if (fileName == null) return;

            if (context.IsFileIncluded(fileName)) return;

            logger.LogInfo("Parse file: " + fileName);

            var code = File.ReadAllLines(fileName);

            context.AddFileIncluded(fileName);

            ParseDocument(sourceCodeNode, code, fileName);
        }

        private void ValidateNodesTree(SourceCodeNode sourceCodeNode)
        {
            var validationContext = new ValidationContext(context);
            sourceCodeNode.ValidateTree(validationContext);
        }

        private void Finalize()
        {
            sourceCodeDocument.Reset();
            logger.Reset();
        }

        private IParsableNode ParseLine(IParsableNode currentNode)
        {
            var node = currentNode.Parse(context);
            if (node == null)
            {
                throw new InvalidOperationException(
                    $"({currentNode}) Parse should return child node or itself.");
            }
            return node;
        }
    }
}