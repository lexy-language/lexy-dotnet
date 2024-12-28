using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public interface IDependantExpression
{
    void LinkPreviousExpression(Expression expression, IParserContext context);
}