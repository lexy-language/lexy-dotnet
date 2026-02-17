using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Language;
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

    public async Task<VerifySuggestions> Keyword(string code, int lineNumber, int column, string name) =>
        await Suggestion(code, lineNumber, column, SymbolKind.Keyword, name, "keyword");

    public async Task<VerifySuggestions> Parameter(string code, int lineNumber, int column, string name, string description) =>
        await Suggestion(code, lineNumber, column, SymbolKind.ParameterVariable, name, description);

    public async Task<VerifySuggestions> Result(string code, int lineNumber, int column, string name, string description) =>
        await Suggestion(code, lineNumber, column, SymbolKind.ResultVariable, name, description);

    public async Task<VerifySuggestions> Suggestion(string code, int lineNumber, int column, Action<VerifyMultipleSuggestion> testHandler)
    {
        var result = await GetSuggestions(code, lineNumber, column);

        var verifyMultipleSuggestion = new VerifyMultipleSuggestion(context, result, testHandler, index++);
        verifyMultipleSuggestion.Verify();

        return this;
    }

    private async Task<SuggestionsResult> GetSuggestions(string code, int lineNumber, int column)
    {
        var result = await serviceProvider.GetSymbols($"test.{index}.lexy", code, true);
        return result.Symbols.GetSuggestions(result.File, new Position(lineNumber, column));
    }

    private async Task<VerifySuggestions> Suggestion(string code, int lineNumber, int column, SymbolKind kind, string name, string description)
    {
        var result = await GetSuggestions(code, lineNumber, column);

        var message = $"All:\n{Format(result.All)}\nFiltered:\n{Format(result.Filtered)}";
        var assertionMessage = $"{index++}: {name} - {kind}\n\n{message}";

        var element = result.Filtered.FirstOrDefault(value => value.Name == name);

        if (element == null) {
            context.Fail("Element not found: " + assertionMessage);
            return this;
        }

        context
            .IsTrue(element.Kind == kind, "Suggestion: " + element + "\n" + assertionMessage)
            .IsTrue(element.Description == description, "Suggestion: " + element + "\n" + assertionMessage);

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
