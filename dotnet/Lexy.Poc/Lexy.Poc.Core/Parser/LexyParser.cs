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

        public LexyParser(IParserContext parserContext, ISourceCodeDocument sourceCodeDocument)
        {
            context = parserContext ?? throw new ArgumentNullException(nameof(parserContext));
            this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
        }

        public ParserResult ParseFile(string fileName, bool throwException = true)
        {
            var code = File.ReadAllLines(fileName);

            return Parse(code, throwException);
        }

        public ParserResult Parse(string[] code, bool throwException = true)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            sourceCodeDocument.SetCode(code);

            var currentIndent = 0;

            IComponent currentComponent = null;

            var componentStack = new Stack<IComponent>();

            while (sourceCodeDocument.HasMoreLines())
            {
                if (!context.ProcessLine())
                {
                    currentIndent = sourceCodeDocument.CurrentLine.Indent();
                    continue;
                }

                var line = sourceCodeDocument.CurrentLine;
                var indent = line.Indent();

                if (indent == 0 && !line.IsComment() && !line.IsEmpty())
                {
                    var root = GetToken(line, context);
                    if (root == null) continue;

                    context.ProcessComponent(root);

                    currentComponent = root;
                    currentIndent = indent;

                    componentStack.Push(currentComponent);

                    continue;
                }

                if (line.IsComment() || line.IsEmpty())
                {
                    currentComponent?.Parse(context);
                    continue;
                }

                if (currentComponent == null)
                {
                    context.Logger.Fail( $"Unexpected line: {line}");
                    continue;
                }

                var indentDifference = currentIndent - indent;
                for (var stackWalkBack = 0; stackWalkBack < indentDifference; stackWalkBack++)
                {
                    currentComponent = componentStack.Pop();
                }

                var component = currentComponent.Parse(context);
                if (component != currentComponent)
                {
                    componentStack.Push(currentComponent);
                    currentComponent = component;
                    currentIndent = indent + 1;
                }
                else
                {
                    currentIndent = indent;
                }
            }

            if (throwException)
            {
                context.Logger.AssertNoErrors();
            }

            return new ParserResult(context.Components);
        }

        private IRootComponent GetToken(Line line, IParserContext context)
        {
            var tokenName = ComponentName.Parse(line, context);

            return tokenName?.Name switch
            {
                null => null,
                TokenValues.FunctionComponent => Function.Parse(tokenName),
                TokenValues.EnumComponent => EnumDefinition.Parse(tokenName),
                TokenValues.ScenarioComponent => Scenario.Parse(tokenName),
                TokenValues.TableComponent => Table.Parse(tokenName),
                _ => InvalidComponent(tokenName, context)
            };
        }

        private IRootComponent InvalidComponent(ComponentName tokenName, IParserContext context)
        {
            var message = $"Unknown keyword: {tokenName.Name}";
            context.Logger.Fail(message);
            return null;
        }
    }
}