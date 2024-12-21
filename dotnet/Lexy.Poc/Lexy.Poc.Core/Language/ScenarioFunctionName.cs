using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.Configuration;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioFunctionName : IComponent
    {
        public string Value { get; private set; }

        public IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            Value = line.Tokens.TokenValue(1);
            return this;
        }

        public override string ToString() => Value;
    }
}