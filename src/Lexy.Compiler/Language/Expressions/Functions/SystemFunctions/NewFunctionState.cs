namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class NewFunctionState
{
    public VariablesMapping Mapping { get; }

    public NewFunctionState(VariablesMapping mapping)
    {
        Mapping = mapping;
    }
}