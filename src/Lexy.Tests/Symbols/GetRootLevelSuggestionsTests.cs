using System.Threading.Tasks;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetRootLevelSuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public async Task ScenarioKeyword()
    {
        await VerifySuggestions(async context =>
            {
                await context.Keyword("s", 1, 2, "scenario");
                await context.Keyword("scen", 1, 2, "scenario");
                await context.Keyword("scen", 1, 4, "scenario");
            }
        );
    }

    [Test]
    public async Task EnumKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Keyword("e", 1, 2, "enum");
            await context.Keyword("enu", 1, 2, "enum");
            await context.Keyword("enu", 1, 3, "enum");
        });
    }

    [Test]
    public async Task TypeKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Suggestion("t", 1, 2, suggestions => suggestions
                .Keyword("type")
                .Keyword("table"));
            await context.Keyword("ty", 1, 3, "type");
            await context.Keyword("typ", 1, 4, "type");
        });
    }

    [Test]
    public async Task TableKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Suggestion("t", 1, 2, suggestions => suggestions
                .Keyword("table")
                .Keyword("type"));
            await context.Keyword("tab", 1, 4, "table");
            await context.Keyword("tabl", 1, 5, "table");
        });
    }

    [Test]
    public async Task FunctionKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Keyword("f", 1, 2, "function");
            await context.Keyword("fun", 1, 4, "function");
            await context.Keyword("functi", 1, 7, "function");
        });
    }

    [Test]
    public async Task IncludeKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Keyword("i", 1, 2, "include");
            await context.Keyword("inc", 1, 4, "include");
            await context.Keyword("inclu", 1, 6, "include");
        });
    }
}
