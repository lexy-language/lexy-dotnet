using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public abstract class SuggestionEdit
{
    public SuggestionsScope Scope { get; }

    protected SuggestionEdit(SuggestionsScope scope)
    {
        Scope = scope;
    }

    public abstract void Update(List<Suggestion> suggestions);
}

public class AddSuggestion : SuggestionEdit
{
    public string Name { get; }
    public SymbolKind Kind { get; }
    public Type Type { get; }

    public AddSuggestion(SuggestionsScope scope, string name, SymbolKind kind, Type type) : base(scope)
    {
        Name = Assert.NotNull(name, nameof(name));
        Kind = kind;
        Type = type;
    }

    public override void Update(List<Suggestion> suggestions)
    {
        suggestions.Add(new Suggestion(Name, Kind, Type));
    }

    public override string ToString() => $"Add: ({Kind}) {Type} {Name} ";
}

public class RemoveSuggestion : SuggestionEdit
{
    public string Name { get; }
    public SymbolKind Kind { get; }

    public RemoveSuggestion(string name, SymbolKind kind) : base(SuggestionsScope.Children)
    {
        Name = Assert.NotNull(name, nameof(name));
        Kind = kind;
    }

    public override void Update(List<Suggestion> suggestions)
    {
        var index = suggestions.FindIndex(where => where.Name == Name);
        if (index >= 0)
        {
            suggestions.RemoveAt(index);
        }
    }

    public override string ToString() => $"Remove: ({Kind}) {Name} ";
}

public class Suggestion
{
    public string Name { get; }
    public SymbolKind Kind { get; }
    public Type Type { get; }

    public Suggestion(string name, SymbolKind kind, Type type = null)
    {
        Name = Assert.NotNull(name, nameof(name));
        Kind = kind;
        Type = type;
    }

    public override string ToString()
    {
        return Type == null
            ? $"{Name} ({Kind})"
            : $"{Type.GetType().Name} {Name} ({Kind})";
    }
}
