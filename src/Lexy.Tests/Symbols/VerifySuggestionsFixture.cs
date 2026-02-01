using System;

namespace Lexy.Tests.Symbols;

public class VerifySuggestionsFixture : ScopedServicesTestFixture
{
    protected void VerifySuggestions(Action<VerifySuggestions> handler)
    {
        Verify.All(context => handler(new VerifySuggestions(ServiceProvider, context)));
    }
}