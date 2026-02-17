using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Tests.Symbols;

public class VerifyMultipleSuggestion
{
    private class Assertion
    {
        public string Name { get; }
        public string Description { get; }
        public SymbolKind Kind { get; }
        public bool Negate { get; }

        public Assertion(string name, string description, SymbolKind kind, bool negate = false)
        {
            Name = name;
            Description = description;
            Kind = kind;
            Negate = negate;
        }

        public override string ToString()
        {
            return Negate
                ? $"'${Name}' ({Kind}) '{Description}'"
                : $"not '{Name}'";
        }
    }

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

    public VerifyMultipleSuggestion Keyword(string name) => Verify(name, SymbolKind.Keyword, "keyword");
    public VerifyMultipleSuggestion NotKeyword(string name) => VerifyNot(name, SymbolKind.Keyword);
    public VerifyMultipleSuggestion Parameter(string name, string description) => Verify(name, SymbolKind.ParameterVariable, description);
    public VerifyMultipleSuggestion Result(string name, string description) => Verify(name, SymbolKind.ResultVariable, description);
    public VerifyMultipleSuggestion Variable(string name, string description) => Verify(name, SymbolKind.Variable, description);
    public VerifyMultipleSuggestion ObjectVariable(string name, string description) => Verify(name, SymbolKind.ObjectVariable, description);

    private VerifyMultipleSuggestion Verify(string name, SymbolKind kind, string description)
    {
        assertions.Add(new Assertion(name, description, kind));
        return this;
    }

    private VerifyMultipleSuggestion VerifyNot(string name, SymbolKind kind)
    {
        assertions.Add(new Assertion(name, null, kind, true));
        return this;
    }

    public void Verify()
    {
        testHandler(this);

        var message = $"All:\n{Format(result.All)}\nFiltered:\n{Format(result.Filtered)}";

        foreach (var assertion in assertions)
        {
            Verify(assertion, message);
        }
    }

    private Expression<Func<Suggestion, bool>> Criteria(Assertion assertion) =>
        value => value.Name == assertion.Name && value.Kind == assertion.Kind;

    private void Verify(Assertion assertion, string message)
    {
        var assertionMessage = $"{parentIndex}.{index++}: {assertion}\n\n{message}";

        var element = result.Filtered.FirstOrDefault(value => value.Name == assertion.Name);

        if (assertion.Negate) {
            parentContext.IsTrue(element == null, "Element not found but shouldn't: " + assertion.Name + "\n" + assertionMessage);
            return;
        }

        if (element == null) {
            parentContext.Fail(" - Element not found: " + assertion.Name + "\n" + assertionMessage);
            return;
        }

        parentContext
            .IsTrue(element.Kind == assertion.Kind, $"Element kind not correct: {element}\n{assertion.Name}\n{assertionMessage}")
            .IsTrue(element.Description == assertion.Description, $"Element description not correct: {element}\n{assertion.Description}\n{assertionMessage}");
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
