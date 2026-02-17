using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class AddSuggestion : SuggestionEdit
{
    public string Name { get; }
    public string Description { get; }
    public SymbolKind Kind { get; }
    public Type Type { get; }

    public AddSuggestion(SuggestionsScope scope, string name, string description, SymbolKind kind, Type type) : base(scope)
    {
        Name = Assert.NotNull(name, nameof(name));
        Description = description;
        Kind = kind;
        Type = type;
    }

    public override void Update(List<Suggestion> suggestions)
    {
        suggestions.Add(new Suggestion(Name, Description, Kind, Type));
    }

    public override string ToString() => $"Add: ({Kind}) {Type} {Name}";
}
