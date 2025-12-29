using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class Mapping
{
    public string VariableName { get; }
    public VariableType VariableType { get; }
    public VariableSource VariableSource { get; }

    public Mapping(string variableName, VariableType variableType, VariableSource variableSource)
    {
        VariableName = variableName;
        VariableType = variableType;
        VariableSource = variableSource;
    }

    public VariableUsage ToUsedVariable(VariableAccess access)
    {
        var variablePath = IdentifierPath.Parse(VariableName);
        return new VariableUsage(variablePath, null, VariableType, VariableSource, access);
    }
}