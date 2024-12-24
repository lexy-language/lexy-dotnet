using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    internal class ExpressionList : IReadOnlyList<Expression>
    {
        private readonly List<Expression> values = new List<Expression>();

        public int Count => values.Count;
        public Expression this[int index] => values[index];

        public IEnumerator<Expression> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Expression expression, IParserContext context)
        {
            if (expression is IDependantExpression childExpression)
            {
                childExpression.LinkPreviousExpression(values.LastOrDefault(), context);
            }
            else
            {
                values.Add(expression);
            }
        }
    }
}