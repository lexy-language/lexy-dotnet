using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class TableHeaders
    {
        public IList<TableHeader> Values { get; } = new List<TableHeader>();

        private TableHeaders(TableHeader[] values)
        {
            Values = values;
        }

        public static TableHeaders Parse(IParserContext context)
        {
            var index = 0;
            var validator = context.ValidateTokens<TableHeaders>();

            if (!validator.Type<TableSeparatorToken>(index).IsValid)
            {
                return null;
            }

            var headers = new List<TableHeader>();
            var tokens = context.CurrentLine.Tokens;
            while (++index < tokens.Length)
            {
                if (!validator
                        .Type<StringLiteralToken>(index)
                        .Type<StringLiteralToken>(index + 1)
                        .Type<TableSeparatorToken>(index + 2)
                        .IsValid)
                {
                    return null;
                }

                var typeName = tokens.TokenValue(index);
                var name = tokens.TokenValue(++index);

                var header = TableHeader.Parse(name, typeName);
                headers.Add(header);

                ++index;
            }

            return new TableHeaders(headers.ToArray());
        }
    }
}