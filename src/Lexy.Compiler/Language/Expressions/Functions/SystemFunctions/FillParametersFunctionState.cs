using Lexy.Compiler.Language.TypeSystem.Objects;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class FillParametersFunctionState
{
    public VariablesMapping Mapping { get; }
    public GeneratedType Type { get; }

    public FillParametersFunctionState(GeneratedType type, VariablesMapping mapping)
    {
        Type = type;
        Mapping = mapping;
    }
}
