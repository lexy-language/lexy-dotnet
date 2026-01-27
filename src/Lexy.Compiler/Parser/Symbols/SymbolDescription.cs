namespace Lexy.Compiler.Parser.Symbols;

public class SymbolDescription
{
    public string Name { get; }
    public string Description { get; }
    public SymbolKind Kind { get; }

    public SymbolDescription(string name, string description, SymbolKind kind)
    {
        Name = name;
        Description = description;
        Kind = kind;
    }

    public override string ToString() => $"{Name} ({Kind}): {Description}";
}
