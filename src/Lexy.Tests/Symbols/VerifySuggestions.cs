using System;
using System.Collections.Generic;
using System.IO;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Tests.Symbols;

public class VerifySuggestions
{
    private readonly IServiceProvider serviceProvider;
    private readonly VerifyContext context;
    private int index;

    public VerifySuggestions(IServiceProvider serviceProvider, VerifyContext context)
    {
        this.serviceProvider = Assert.NotNull(serviceProvider, nameof(serviceProvider));
        this.context = Assert.NotNull(context, nameof(context));
    }

    public VerifySuggestions Keyword(string code, int lineNumber, int column, string name) =>
        Suggestion(code, lineNumber, column, SymbolKind.Keyword, name);

    public VerifySuggestions Parameter(string code, int lineNumber, int column, string name) =>
        Suggestion(code, lineNumber, column, SymbolKind.ParameterVariable, name);

    public VerifySuggestions Result(string code, int lineNumber, int column, string name) =>
        Suggestion(code, lineNumber, column, SymbolKind.ResultVariable, name);

    public VerifySuggestions Suggestion(string code, int lineNumber, int column, Action<VerifyMultipleSuggestion> testHandler)
    {
        var result = GetSuggestions(code, lineNumber, column);

        var verifyMultipleSuggestion = new VerifyMultipleSuggestion(context, result, testHandler, index++);
        verifyMultipleSuggestion.Verify();

        return this;
    }

    private SuggestionsResult GetSuggestions(string code, int lineNumber, int column)
    {
        var symbols = serviceProvider.GetSymbols($"test.{index}.lexy", code, true);
        var result = symbols.Result.Symbols.GetSuggestions($"test.{index}.lexy", new Position(lineNumber, column));

        return result;
    }

    private VerifySuggestions Suggestion(string code, int lineNumber, int column, SymbolKind kind, string name)
    {
        var result = GetSuggestions(code, lineNumber, column);

        var message = $"All:\n{Format(result.All)}\nFiltered:\n{Format(result.Filtered)}";
        var assertionMessage = $"{index++}: {name} - {kind}\n\n{message}";

        context.Collection(result.Filtered, verifySuggestions => verifySuggestions
            .Any(value => value.Name == name && value.Kind == kind, assertionMessage));

        return this;
    }

    private static string Format(IEnumerable<Suggestion> suggestions)
    {
        var writer = new StringWriter();
        foreach (var suggestion in suggestions)
        {
            writer.WriteLine("  - " + suggestion);
        }
        return writer.ToString();
    }
}
