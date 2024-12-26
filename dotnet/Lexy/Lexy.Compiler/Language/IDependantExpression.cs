using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public interface IDependantExpression
    {
        void LinkPreviousExpression(Expression expression, IParserContext context);
    }
}