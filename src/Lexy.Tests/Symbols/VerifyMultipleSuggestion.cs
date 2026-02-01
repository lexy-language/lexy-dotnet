using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Tests.Symbols;

public class VerifyMultipleSuggestion
{
    private record Assertion(string Name, SymbolKind Kind, bool Negate = false);

    private readonly VerifyContext parentContext;
    private readonly List<Assertion> assertions;
    private readonly SuggestionsResult result;
    private readonly Action<VerifyMultipleSuggestion> testHandler;
    private readonly int parentIndex;

    private int index;

    public VerifyMultipleSuggestion(VerifyContext parentContext, SuggestionsResult result, Action<VerifyMultipleSuggestion> testHandler, int parentIndex)
    {
        this.parentContext = Assert.NotNull(parentContext, nameof(parentContext));
        this.result = Assert.NotNull(result, nameof(result));
        this.testHandler = Assert.NotNull(testHandler, nameof(testHandler));
        this.parentIndex = parentIndex;

        assertions = new List<Assertion>();
    }

    public VerifyMultipleSuggestion Keyword(string name) => Verify(name, SymbolKind.Keyword);
    public VerifyMultipleSuggestion NotKeyword(string name) => VerifyNot(name, SymbolKind.Keyword);
    public VerifyMultipleSuggestion Parameter(string name) => Verify(name, SymbolKind.ParameterVariable);
    public VerifyMultipleSuggestion Result(string name) => Verify(name, SymbolKind.ResultVariable);
    public VerifyMultipleSuggestion Variable(string name) => Verify(name, SymbolKind.Variable);
    public VerifyMultipleSuggestion ObjectVariable(string name) => Verify(name, SymbolKind.ObjectVariable);

    private VerifyMultipleSuggestion Verify(string name, SymbolKind kind)
    {
        assertions.Add(new Assertion(name, kind));
        return this;
    }

    private VerifyMultipleSuggestion VerifyNot(string name, SymbolKind kind)
    {
        assertions.Add(new Assertion(name, kind, true));
        return this;
    }

    public void Verify()
    {
        testHandler(this);

        var message = $"All:\n{Format(result.All)}\nFiltered:\n{Format(result.Filtered)}";

        parentContext.Collection(result.Filtered, verifySuggestions =>
        {
            foreach (var assertion in assertions)
            {
                Verify(assertion, message, verifySuggestions);
            }
        });
    }

    private Expression<Func<Suggestion, bool>> Criteria(Assertion assertion) =>
        value => value.Name == assertion.Name && value.Kind == assertion.Kind;

    private void Verify(Assertion assertion, string message, VerifyCollectionContext<Suggestion> verifySuggestions)
    {
        var assertionMessage = $"{parentIndex}.{index++}: {assertion}\n\n{message}";
        if (!assertion.Negate)
        {
            verifySuggestions.Any(Criteria(assertion), assertionMessage);
        }
        else
        {
            verifySuggestions.None(Criteria(assertion), assertionMessage);
        }
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
