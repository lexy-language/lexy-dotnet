namespace Lexy.Compiler.Language.Expressions;

public interface IParentExpression : INode
{
    void LinkChildExpression(IChildExpression expression);
}