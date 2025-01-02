using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

public interface IDependantExpression
{
    void LinkPreviousExpression(Expression expression, IParseLineContext context);
}