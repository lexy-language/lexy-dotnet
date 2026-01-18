using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class Mapping
{
    public string VariableName { get; }
    public Type Type { get; }
    public VariableSource VariableSource { get; }

    public Mapping(string variableName, Type type, VariableSource variableSource)
    {
        VariableName = variableName;
        Type = type;
        VariableSource = variableSource;
    }

    public VariableUsage ToUsedVariable(VariableAccess access)
    {
        var variablePath = IdentifierPath.Parse(VariableName);
        return new VariableUsage(variablePath, null, Type, VariableSource, access);
    }
}