using System.Threading.Tasks;
using NUnit.Framework;

namespace Lexy.Tests.Symbols;

public class GetScenarioSuggestionsTests : VerifySuggestionsFixture
{
    [Test]
    public async Task ScenarioKeyword()
    {
        await VerifySuggestions(async context =>
        {
            await context.Keyword(@"scenario Name
  p", 2, 3, "parameters");
            await context.Keyword(@"scenario Name
  res", 2, 5, "results");
            await context.Keyword(@"scenario Name
  va", 2, 4, "validationTable");
        });

        //todo add variables
    }
}
