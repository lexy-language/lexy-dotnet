using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class BooleanLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestBooleanTrueLiteral()
    {
        ServiceProvider
            .Tokenize(@"   true")
            .Count(1)
            .Boolean(0, true)
            .Assert();
    }

    [Test]
    public void TestBooleanFalseLiteral()
    {
        ServiceProvider
            .Tokenize(@"   false")
            .Count(1)
            .Boolean(0, false)
            .Assert();
    }
}