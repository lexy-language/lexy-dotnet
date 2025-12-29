using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language;

public class VariableReference
{
    public IdentifierPath Path { get; }
    public VariableSource Source { get; }
    public VariableType ComponentType { get; }
    public VariableType VariableType { get; }

    public VariableReference(IdentifierPath path, VariableType componentType,
        VariableType variableType, VariableSource source)
    {
        Path = path;
        ComponentType = componentType;
        VariableType = variableType;
        Source = source;
    }

    public override string ToString()
    {
        return Path.ToString();
    }
}