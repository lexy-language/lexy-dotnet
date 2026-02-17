using System;
using System.Collections.Generic;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Symbols;

public class Suggestions
{
    private readonly List<SuggestionEdit> values = new();
    private readonly SuggestionsScope scope;

    private Suggestions(SuggestionsScope scope = SuggestionsScope.Children)
    {
        this.scope = scope;
    }

    public static SuggestionEdit[] Edit(SuggestionsScope scope, Action<Suggestions> handler)
    {
        var builder = new Suggestions(scope);
        handler(builder);
        return builder.Edit();
    }

    public static SuggestionEdit[] Edit(Action<Suggestions> handler)
    {
        var builder = new Suggestions();
        handler(builder);
        return builder.Edit();
    }

    private SuggestionEdit[] Edit() => values.ToArray();

    public Suggestions Keyword(string name) => Add(name, "keyword", SymbolKind.Keyword);
    public Suggestions Parameter(string name, Type type, string description = null) => Add(name, description, SymbolKind.ParameterVariable, type);
    public Suggestions Result(string name, Type type, string description = null) => Add(name, description, SymbolKind.ResultVariable, type);
    public Suggestions Variable(string name, Type type, string description = null) => Add(name, description, SymbolKind.Variable, type);
    public Suggestions TypeVariable(string name, Type type, string description = null) => Add(name, description, SymbolKind.ObjectVariable, type);

    public Suggestions RemoveKeyword(string name) => Remove(name, SymbolKind.Keyword);

    private Suggestions Add(string name, string description, SymbolKind kind, Type type = null)
    {
        values.Add(new AddSuggestion(scope, name, description, kind, type));
        return this;
    }

    private Suggestions Remove(string name, SymbolKind kind)
    {
        values.Add(new RemoveSuggestion(name, kind));
        return this;
    }
}
