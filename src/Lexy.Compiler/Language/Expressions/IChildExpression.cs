using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

public interface IChildExpression : INode
{
    bool ValidateParentExpression(IParentExpression expression, IParseLineContext context);
}

public interface IHasVariableReference : INode
{
    VariableReference Variable { get; }
}