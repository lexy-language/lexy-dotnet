using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectError : IComponent
    {
        public string Message { get; private set; }
        public bool HasValue => Message != null;

        public IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.IsEmpty()) return this;

            var valid = context.ValidateTokens<ScenarioExpectError>()
                .Count(2)
                .Keyword(0)
                .QuotedString(1)
                .IsValid;

            if (!valid) return this;

            Message = line.Tokens.TokenValuesFrom(1);
            return this;
        }
    }
}