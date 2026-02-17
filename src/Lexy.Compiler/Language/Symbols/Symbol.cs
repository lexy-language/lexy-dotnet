using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class Symbol
{
    public string Name { get; }
    public string Description { get; }
    public SymbolKind Kind { get; }
    public Signatures Signatures { get; }
    public SourceReference Reference { get; }

    public Symbol(SourceReference reference, string name, string description, SymbolKind kind, Signatures signatures = null)
    {
        Reference = Assert.NotNull(reference, nameof(reference));
        Name = Assert.NotNull(name, nameof(name));
        Description = description;
        Kind = kind;
        Signatures = signatures;
    }

    public override string ToString()
    {
        var value = !string.IsNullOrEmpty(Description) ? $"{Name}: {Description}" : Name;

        return Signatures != null ? $"value - {Signatures}" : value;
    }
}
