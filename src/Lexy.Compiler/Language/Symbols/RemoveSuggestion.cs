using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

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

    public override string ToString() => $"Remove: ({Kind}) {Name}";
}