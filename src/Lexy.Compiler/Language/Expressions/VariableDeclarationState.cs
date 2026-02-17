using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Expressions;

public class VariableDeclarationState
{
    public Type Type { get; }

    public VariableDeclarationState(Type type)
    {
        Type = type;
    }
}