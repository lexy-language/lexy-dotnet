using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetFunctionSuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public void Keywords()
    {
        const string code = "function Name\n  ";

        VerifySuggestions(context => context
            .Suggestion(code, 2, 3, verify => verify
                .NotKeyword("function")
                .NotKeyword("enum")
                .NotKeyword("type")
                .NotKeyword("table")
                .NotKeyword("scenario")
                .NotKeyword("include")
            )
            .Suggestion(code + "p", 2, 4, verify => verify
                .Keyword("parameters")
            )
            .Suggestion(code + "res", 2, 5, verify => verify
                .Keyword("results")
            )
            .Suggestion(code + "i", 2, 4, verify => verify
                .Keyword("if")
                .NotKeyword("elseif")
                .NotKeyword("else")
            )
            .Suggestion(code + "if true\n  e", 3, 3, verify => verify
                .NotKeyword("if")
                .Keyword("elseif")
                .Keyword("else")
            )
            .Suggestion(code + "if true\n  var a = 6\n  e", 4, 3, verify => verify
                .NotKeyword("if")
                .Keyword("elseif")
                .Keyword("else")
            )
            .Suggestion(code + "s", 2, 4, verify => verify
                .Keyword("switch")
                .NotKeyword("case")
                .NotKeyword("default")
            )
            .Suggestion(code + "switch 5\n    c", 3, 6, verify => verify
                .NotKeyword("switch")
                .Keyword("case")
                .NotKeyword("default")
            )
            .Suggestion(code + "switch 5\n    d", 3, 6, verify => verify
                 .NotKeyword("switch")
                 .NotKeyword("case")
                 .Keyword("default")
             )
        );
    }

    [Test]
    public void Parameter()
    {
        const string code = @"function Name
  parameters
    number Value1
  var value = Val";

        VerifySuggestions(context => context
            .Suggestion(code, 4, 16, context => context
                .Parameter("Value1")
            )
        );
    }

    [Test]
    public void Result()
    {
        const string code = @"function Name
  results
    number Result1
  Resu";

        VerifySuggestions(context => context
            .Suggestion(code, 4, 7, context => context
                .Result("Result1")
            )
        );
    }

    [Test]
    public void Variable()
    {
        const string code = @"function Name
  var value1 = 5
  val";

        VerifySuggestions(context => context
            .Suggestion(code, 3, 5, context => context
                .Variable("value1")
            ));
    }

    [Test]
    public void ObjectVariable()
    {
        const string code = @"type Object
  number Value

function Name
  Object value1
  value1.Val";

        VerifySuggestions(context => context
            .Suggestion(code, 6, 12, context => context
                .ObjectVariable("Value")
            )
        );
    }

    [Test]
    public void ObjectVariableNoHint()
    {
        const string code = @"type Object
  number Value
  string Member

function Name
  Object value1
  value1.";

        VerifySuggestions(context => context
            .Suggestion(code, 7, 9, context => context
                .ObjectVariable("Value")
                .ObjectVariable("Member")
            )
        );
    }
}
