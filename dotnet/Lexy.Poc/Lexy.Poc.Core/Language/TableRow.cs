using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class TableRow
    {
        public IList<ILiteralToken> Values { get; } = new List<ILiteralToken>();

        private TableRow(ILiteralToken[] values)
        {
            Values = values;
        }

        public static TableRow Parse(ParserContext context)
        {
            var index = 0;
            var validator = context.ValidateTokens<TableRow>();

            if (!validator.Type<TableSeparatorToken>(index).IsValid)
            {
                return null;
            }

            var tokens = new List<ILiteralToken>();
            while (++index < context.CurrentLine.Tokens.Length)
            {
                if (!validator
                    .IsLiteralToken(index)
                    .Type<TableSeparatorToken>(index + 1)
                    .IsValid)
                {
                    return null;
                }

                var token = context.CurrentLine.LiteralToken(index++);
                tokens.Add(token);
            }

            return new TableRow(tokens.ToArray());
        }
    }
}