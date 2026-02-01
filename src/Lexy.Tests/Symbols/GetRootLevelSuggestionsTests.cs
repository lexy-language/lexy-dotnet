using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class VerifySuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public void ScenarioKeyword()
    {
        VerifySuggestions(context => context
            .Keyword("s", 1, 2, "scenario")
            .Keyword("scen", 1, 2, "scenario")
            .Keyword("scen", 1, 4, "scenario")
        );
    }

    [Test]
    public void EnumKeyword()
    {
        VerifySuggestions(context => context
            .Keyword("e", 1, 2, "enum")
            .Keyword("enu", 1, 2, "enum")
            .Keyword("enu", 1, 3, "enum")
        );
    }

    [Test]
    public void TypeKeyword()
    {
        VerifySuggestions(context => context
            .Suggestion("t", 1, 2, suggestions => suggestions
                .Keyword("type")
                .Keyword("table"))
            .Keyword("ty", 1, 3, "type")
            .Keyword("typ", 1, 4, "type")
        );
    }

    [Test]
    public void TableKeyword()
    {
        VerifySuggestions(context => context
            .Suggestion("t", 1, 2, suggestions => suggestions
                .Keyword("table")
                .Keyword("type"))
            .Keyword("tab", 1, 4, "table")
            .Keyword("tabl", 1, 5, "table")
        );
    }

    [Test]
    public void FunctionKeyword()
    {
        VerifySuggestions(context => context
            .Keyword("f", 1, 2, "function")
            .Keyword("fun", 1, 4, "function")
            .Keyword("functi", 1, 7, "function")
        );
    }

    [Test]
    public void IncludeKeyword()
    {
        VerifySuggestions(context => context
            .Keyword("i", 1, 2, "include")
            .Keyword("inc", 1, 4, "include")
            .Keyword("inclu", 1, 6, "include")
        );
    }
}
