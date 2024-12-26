using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class TableRow : Node
    {
        public IList<ILiteralToken> Values { get; } = new List<ILiteralToken>();

        private TableRow(ILiteralToken[] values, SourceReference reference) : base(reference)
        {
            Values = values;
        }

        public static TableRow Parse(IParserContext context)
        {
            var index = 0;
            var validator = context.ValidateTokens<TableRow>();

            if (!validator.Type<TableSeparatorToken>(index).IsValid)
            {
                return null;
            }

            var tokens = new List<ILiteralToken>();
            var currentLineTokens = context.CurrentLine.Tokens;
            while (++index < currentLineTokens.Length)
            {
                if (!validator
                    .IsLiteralToken(index)
                    .Type<TableSeparatorToken>(index + 1)
                    .IsValid)
                {
                    return null;
                }

                var token = currentLineTokens.LiteralToken(index++);
                tokens.Add(token);
            }

            return new TableRow(tokens.ToArray(), context.LineStartReference());
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