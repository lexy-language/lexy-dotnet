using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionCode : IComponent
    {
        public IList<Expression> Lines { get; } = new List<Expression>();

        public IComponent Parse(IParserContext context)
        {
            if (context.CurrentLine.IsComment())
            {
                return this;
            }
            if (context.CurrentLine.IsEmpty())
            {
                return this;
            }

            var valid = context.ValidateTokens<FunctionCode>()
                .CountMinimum(1)
                .IsValid;

            if (!valid) return null;

            var expression = Expression.Parse(context, context.CurrentLine.Tokens);

            Lines.Add(expression);
            return this;
        }
    }
}