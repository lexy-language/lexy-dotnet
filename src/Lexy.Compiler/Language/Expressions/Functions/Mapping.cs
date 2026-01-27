using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class Mapping
{
    public SourceReference Reference { get; }
    public string VariableName { get; }
    public Type Type { get; }
    public VariableSource VariableSource { get; }

    public Mapping(SourceReference reference, string variableName, Type type, VariableSource variableSource)
    {
        Reference = reference;
        VariableName = variableName;
        Type = type;
        VariableSource = variableSource;
    }

    public VariableUsage ToUsedVariable(VariableAccess access)
    {
        var identifierPath = IdentifierPath.Parse(VariableName);
        return new VariableUsage(Reference, identifierPath, null, Type, VariableSource, access);
    }
}
