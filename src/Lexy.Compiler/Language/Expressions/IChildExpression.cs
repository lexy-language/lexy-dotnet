using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Expressions;

public interface IChildExpression : INode
{
    bool ValidateParentExpression(IParentExpression expression, IParseLineContext context);
}