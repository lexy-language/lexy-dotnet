using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Scenarios;

public class AssignmentDefinitionState
{
    public Type Type { get; }

    public AssignmentDefinitionState(Type type)
    {
        Type = type;
    }
}