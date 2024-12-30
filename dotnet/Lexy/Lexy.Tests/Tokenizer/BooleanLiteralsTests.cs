using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class BooleanLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestBooleanTrueLiteral()
    {
        ServiceProvider
            .Tokenize(@"   true")
            .ValidateTokens()
            .Count(1)
            .Boolean(0, true)
            .Assert();
    }

    [Test]
    public void TestBooleanFalseLiteral()
    {
        ServiceProvider
            .Tokenize(@"   false")
            .ValidateTokens()
            .Count(1)
            .Boolean(0, false)
            .Assert();
    }
}