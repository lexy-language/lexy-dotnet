using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language;

public class VariableReference
{
    public IdentifierPath Path { get; }
    public VariableSource Source { get; }
    public Type ComponentType { get; }
    public Type Type { get; }

    public VariableReference(IdentifierPath path, Type componentType,
        Type type, VariableSource source)
    {
        Path = path;
        ComponentType = componentType;
        Type = type;
        Source = source;
    }

    public override string ToString()
    {
        return Path.ToString();
    }
}