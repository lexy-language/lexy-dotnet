using System.Threading.Tasks;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetFunctionSuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public async Task Keywords()
    {
        const string code = "function Name\n  ";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 2, 3, verify => verify
                .NotKeyword("function")
                .NotKeyword("enum")
                .NotKeyword("type")
                .NotKeyword("table")
                .NotKeyword("scenario")
                .NotKeyword("include")
            );
            await context.Suggestion(code + "p", 2, 4, verify => verify
                .Keyword("parameters")
            );
            await context.Suggestion(code + "res", 2, 5, verify => verify
                .Keyword("results")
            );
            await context.Suggestion(code + "i", 2, 4, verify => verify
                .Keyword("if")
                .NotKeyword("elseif")
                .NotKeyword("else")
            );
            await context.Suggestion(code + "if true\n  e", 3, 3, verify => verify
                .NotKeyword("if")
                .Keyword("elseif")
                .Keyword("else")
            );
            await context.Suggestion(code + "if true\n  var a = 6\n  e", 4, 3, verify => verify
                .NotKeyword("if")
                .Keyword("elseif")
                .Keyword("else")
            );
            await context.Suggestion(code + "s", 2, 4, verify => verify
                .Keyword("switch")
                .NotKeyword("case")
                .NotKeyword("default")
            );
            await context.Suggestion(code + "switch 5\n    c", 3, 6, verify => verify
                .NotKeyword("switch")
                .Keyword("case")
                .NotKeyword("default")
            );
            await context.Suggestion(code + "switch 5\n    d", 3, 6, verify => verify
                .NotKeyword("switch")
                .NotKeyword("case")
                .Keyword("default")
            );
        });
    }

    [Test]
    public async Task Parameter()
    {
        const string code = @"function Name
  parameters
    number Value1
  var value = Val";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 4, 16, context => context
                .Parameter("Value1", "parameter: number")
            );
        });
    }

    [Test]
    public async Task Result()
    {
        const string code = @"function Name
  results
    number Result1
  Resu";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 4, 7, context => context
                .Result("Result1", "result: number")
            );
        });
    }

    [Test]
    public async Task Variable()
    {
        const string code = @"function Name
  var value1 = 5
  val";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 3, 5, context => context
                .Variable("value1", "variable: number")
            );
        });
    }

    [Test]
    public async Task ObjectVariable()
    {
        const string code = @"type Object
  number Value

function Name
  Object value1
  value1.Val";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 6, 12, context => context
                .ObjectVariable("Value", "variable: number")
            );
        });
    }

    [Test]
    public async Task ObjectVariableNoHint()
    {
        const string code = @"type Object
  number Value
  string Member

function Name
  Object value1
  value1.";

        await VerifySuggestions(async context =>
        {
            await context.Suggestion(code, 7, 9, context => context
                .ObjectVariable("Value", "variable: number")
                .ObjectVariable("Member",  "variable: string")
            );
        });
    }
}
