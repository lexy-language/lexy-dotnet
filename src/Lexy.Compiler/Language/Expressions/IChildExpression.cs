using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

public interface IChildExpression : INode
{
    bool ValidatePreviousExpression(IParentExpression expression, IParseLineContext context);
}