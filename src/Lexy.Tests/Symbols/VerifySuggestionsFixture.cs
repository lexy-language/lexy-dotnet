using System;
using System.Threading.Tasks;

namespace Lexy.Tests.Symbols;

public class VerifySuggestionsFixture : ScopedServicesTestFixture
{
    protected async Task VerifySuggestions(Func<VerifySuggestions, Task> handler)
    {
        await Verify.All(async context => await handler(new VerifySuggestions(ServiceProvider, context)));
    }
}
