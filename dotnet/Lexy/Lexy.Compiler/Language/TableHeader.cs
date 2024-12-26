using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public class TableHeader : Node
    {
        public IList<ColumnHeader> Values { get; } = new List<ColumnHeader>();

        private TableHeader(ColumnHeader[] values, SourceReference reference) : base(reference)
        {
            Values = values;
        }

        public static TableHeader Parse(IParserContext context)
        {
            var index = 0;
            var validator = context.ValidateTokens<TableHeader>();

            if (!validator.Type<TableSeparatorToken>(index).IsValid)
            {
                return null;
            }

            var headers = new List<ColumnHeader>();
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
                var reference = context.TokenReference(index);

                var header = ColumnHeader.Parse(name, typeName, reference);
                headers.Add(header);

                ++index;
            }

            return new TableHeader(headers.ToArray(), context.LineStartReference());
        }

        public override IEnumerable<INode> GetChildren() => Values;

        protected override void Validate(IValidationContext context)
        {
        }

        public ColumnHeader Get(string name) => Values.FirstOrDefault(value => value.Name == name);
    }
}