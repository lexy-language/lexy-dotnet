namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class FillParametersFunctionState
{
    public VariablesMapping Mapping { get; }

    public FillParametersFunctionState(VariablesMapping mapping)
    {
        Mapping = mapping;
    }
}