namespace Lexy.Compiler.Language.Expressions;

public interface IHasVariableReference : INode
{
    string Path { get; }
    VariableReference Variable { get; }
}
