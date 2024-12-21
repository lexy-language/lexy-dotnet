using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectRootErrors : IComponent
    {
        private readonly IList<string> messages = new List<string>();

        public IEnumerable<string> Messages => messages;

        public bool HasValues => messages.Count > 0;

        public IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.IsEmpty()) return this;

            var valid = context.ValidateTokens<ScenarioExpectError>()
                .Count(1)
                .QuotedString(0)
                .IsValid;

            if (!valid) return this;

            messages.Add(line.Tokens.TokenValuesFrom(0));
            return this;
        }
    }
}