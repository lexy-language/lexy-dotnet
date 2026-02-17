using Lexy.Compiler.Language.TypeSystem;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class Suggestion
{
    public string Name { get; }
    public string Description { get; }
    public SymbolKind Kind { get; }
    public Type Type { get; }

    public Suggestion(string name, string description, SymbolKind kind, Type type = null)
    {
        Name = Assert.NotNull(name, nameof(name));
        Description = description;
        Kind = kind;
        Type = type;
    }

    public override string ToString()
    {
        var suffix = Description != null ? $": ${Description}" : "";
        return Type == null
            ? $"{Name} ({Kind}){suffix}"
            : $"{Type.GetType().Name} {Name} ({Kind}){suffix}";
    }
}
