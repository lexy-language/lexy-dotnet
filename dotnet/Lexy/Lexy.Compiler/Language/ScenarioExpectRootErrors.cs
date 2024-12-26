using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectRootErrors : ParsableNode
    {
        private readonly IList<string> messages = new List<string>();

        public IEnumerable<string> Messages => messages;

        public bool HasValues => messages.Count > 0;

        public ScenarioExpectRootErrors(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;

            if (line.IsEmpty()) return this;

            var valid = context.ValidateTokens<ScenarioExpectError>()
                .Count(1)
                .QuotedString(0)
                .IsValid;

            if (!valid) return this;

            messages.Add(line.Tokens.Token<QuotedLiteralToken>(0).Value);
            return this;
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
        }
    }
}