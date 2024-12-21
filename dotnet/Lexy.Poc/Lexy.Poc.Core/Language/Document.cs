using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Document : RootComponent
    {
        public override string ComponentName => "Document";

        public override IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.Tokens.IsComment() || line.IsEmpty()) return this;
            if (line.Indent() > 0)
            {
                context.Logger.Fail( $"Unexpected line: {line}");
                return this;
            }

            var tokenName = Parser.ComponentName.Parse(line, context);
            var rootComponent = tokenName?.Name switch
            {
                null => null,
                TokenValues.FunctionComponent => Function.Parse(tokenName),
                TokenValues.EnumComponent => EnumDefinition.Parse(tokenName),
                TokenValues.ScenarioComponent => Scenario.Parse(tokenName),
                TokenValues.TableComponent => Table.Parse(tokenName),
                _ => InvalidComponent(tokenName, context)
            };

            if (rootComponent != null)
            {
                context.ProcessComponent(rootComponent);
            }

            return rootComponent ?? this;
        }

        private IRootComponent InvalidComponent(ComponentName tokenName, IParserContext context)
        {
            var message = $"Unknown keyword: {tokenName.Name}";
            context.Logger.Fail(message);
            return null;
        }
    }
}