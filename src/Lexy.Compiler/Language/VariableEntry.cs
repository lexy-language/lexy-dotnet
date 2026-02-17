using System;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language;

public class VariableEntry
{
    public string Name { get; }
    public Type Type { get; }
    public VariableSource VariableSource { get; }
    public SourceReference Reference { get; }

    public VariableEntry(string name, Type type, VariableSource variableSource, SourceReference reference = null)
    {
        Name = name;
        Type = type;
        VariableSource = variableSource;
        Reference = reference;
    }

    public string ToString()
    {
        switch (VariableSource) {
            case VariableSource.Parameters:
                return $"parameter: {Type}";
            case VariableSource.Results:
                return $"result: {Type}";
            case VariableSource.Code:
                return $"variable: {Type}";
            case VariableSource.Type:
                return TypeSymbol();
            default:
                throw new InvalidOperationException($"VariableEntry: {VariableSource}");
        }
    }

    private string TypeSymbol()
    {
        if (Type is EnumType)
        {
            return $"enum member: {Type}";
        }
        return Type is GeneratedType
            ? $"type: {Type}"
            : $"variable: {Type}";
    }
}
