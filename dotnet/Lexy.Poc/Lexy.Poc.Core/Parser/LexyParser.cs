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

            return Parse(code, Path.GetFileName(fileName), throwException);
        }

        public ParserResult Parse(string[] code, string fileName, bool throwException = true)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            sourceCodeDocument.SetCode(code, fileName);

            var currentIndent = 0;
            var document = new Document(context.DocumentReference());
            var nodes = new ParsableNodeArray(document);

            while (sourceCodeDocument.HasMoreLines())
            {
                if (!context.ProcessLine())
                {
                    currentIndent = sourceCodeDocument.CurrentLine.Indent();
                    continue;
                }

                var line = sourceCodeDocument.CurrentLine;
                if (line.IsComment() || line.IsEmpty())
                {
                    continue;
                }

                var indent = line.Indent();
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

            sourceCodeDocument.Reset();
            logger.Reset();

            var validationContext = new ValidationContext(context);
            document.ValidateTree(validationContext);

            if (throwException)
            {
                logger.AssertNoErrors();
            }

            return new ParserResult(context.Nodes);
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