using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetScenarioSuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public void ScenarioKeyword()
    {
        VerifySuggestions(context => context
            .Keyword(@"scenario Name
  p", 2, 3, "parameters")
            .Keyword(@"scenario Name
  res", 2, 5, "results")
            .Keyword(@"scenario Name
  va", 2, 4, "validationTable")
        );
    }
}
