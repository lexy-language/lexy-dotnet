namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class ExtractResultsFunctionState
{
    public VariablesMapping Mapping { get; }

    public ExtractResultsFunctionState(VariablesMapping mapping)
    {
        Mapping = mapping;
    }
}