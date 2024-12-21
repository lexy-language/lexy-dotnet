using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectError : IComponent
    {
        public string Message { get; private set; }
        public bool HasValue => Message != null;
        public bool Root { get; private set; }

        public IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.IsEmpty()) return this;

            switch (line.Tokens.Length)
            {
                case 2:
                    return Validate2Tokens(context, line);
                case 3:
                    return Validate3Tokens(context, line);

                default:
                    context.Logger.Fail("Invalid number of tokens: " + line.Tokens.Length + ". Should be 2 or 3.");
                    return this;
            }
        }

        private IComponent Validate2Tokens(IParserContext context, Line line)
        {
            var valid = context.ValidateTokens<ScenarioExpectError>()
                .Keyword(0)
                .QuotedString(1)
                .IsValid;

            if (!valid) return this;

            Message = line.TokenValuesFrom(1);
            return this;
        }

        private IComponent Validate3Tokens(IParserContext context, Line line)
        {
            var valid = context.ValidateTokens<ScenarioExpectError>()
                .Keyword(0)
                .StringLiteral(1)
                .QuotedString(2)
                .IsValid;

            if (!valid) return this;

            var tokenValue = line.TokenValue(1);
            if (tokenValue != "root")
            {
                context.Logger.Fail("Invalid token at 1: " + tokenValue + ". Should be root");
            }
            else
            {
                Root = true;
                Message = line.TokenValuesFrom(2);
            }

            return this;
        }

    }
}