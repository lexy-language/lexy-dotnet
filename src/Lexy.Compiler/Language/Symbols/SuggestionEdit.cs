using System.Collections.Generic;

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