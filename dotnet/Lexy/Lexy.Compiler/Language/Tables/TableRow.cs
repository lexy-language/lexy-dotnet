using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language
{
    public class TableRow : Node
    {
        public IList<Expression> Values { get; }

        private TableRow(Expression[] values, SourceReference reference) : base(reference)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public static TableRow Parse(IParserContext context)
        {
            var index = 0;
            var validator = context.ValidateTokens<TableRow>();

            if (!validator.Type<TableSeparatorToken>(index).IsValid)
            {
                return null;
            }

            var tokens = new List<Expression>();
            var currentLineTokens = context.CurrentLine.Tokens;
            while (++index < currentLineTokens.Length)
            {
                var valid = !validator
                    .IsLiteralToken(index)
                    .Type<TableSeparatorToken>(index + 1)
                    .IsValid;

                if (valid)
                {
                    return null;
                }

                var reference = context.TokenReference(index);
                var token = currentLineTokens.Token<Token>(index++);
                var expression = ExpressionFactory.Parse(context.SourceCode.File, new TokenList(new[] { token }),
                    context.CurrentLine);
                if (context.Failed(expression, reference)) return null;

                tokens.Add(expression.Result);
            }

            return new TableRow(tokens.ToArray(), context.LineStartReference());
        }

        public override IEnumerable<INode> GetChildren()
        {
            return Values.ToList();
        }

        protected override void Validate(IValidationContext context)
        {
        }
    }
}