using System;
using System.Collections.Generic;
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

            return Parse(code, throwException);
        }

        public ParserResult Parse(string[] code, bool throwException = true)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            sourceCodeDocument.SetCode(code);

            var currentIndent = 0;
            var componentStack = new Stack<IComponent>();
            IComponent currentComponent = new Document();

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
                    currentComponent?.Parse(context);
                    continue;
                }

                var indent = line.Indent();

                currentComponent = WalkBackOnCallStackByIndent(indent, currentIndent, componentStack, currentComponent);

                var component = ParseLine(currentComponent);

                componentStack.Push(currentComponent);
                currentComponent = component;
                currentIndent = indent + 1;
            }

            if (throwException)
            {
                logger.AssertNoErrors();
            }

            return new ParserResult(context.Components);
        }

        private IComponent ParseLine(IComponent currentComponent)
        {
            var component = currentComponent.Parse(context);
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"({currentComponent}) Parse should return child component or itself.");
            }
            return component;
        }

        private static IComponent WalkBackOnCallStackByIndent(int indent, int currentIndent, Stack<IComponent> componentStack,
            IComponent currentComponent)
        {
            var indentDifference = currentIndent - indent;
            for (var stackWalkBack = 0; stackWalkBack < indentDifference; stackWalkBack++)
            {
                currentComponent = componentStack.Pop();
            }

            return currentComponent;
        }
    }
}