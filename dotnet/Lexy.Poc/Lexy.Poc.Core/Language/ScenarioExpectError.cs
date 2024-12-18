using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectError : IComponent
    {
        public string Message { get; private set; }
        public bool HasValue { get => Message != null; }

        public IComponent Parse(ParserContext context)
        {
            if (context.CurrentLine.IsEmpty()) return this;

            context.ValidateTokens<ScenarioExpectError>()
                .Count(2)
                .Keyword(0)
                .QuotedString(1);

            Message = context.CurrentLine.TokenValuesFrom(1);
            return this;
        }
    }
}