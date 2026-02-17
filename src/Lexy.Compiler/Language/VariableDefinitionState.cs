using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language;

public class VariableDefinitionState
{
    public Type Type { get; }

    public VariableDefinitionState(Type type)
    {
        Type = type;
    }
}