using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class StringLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestQuotedLiteral()
    {
        ServiceProvider
            .Tokenize(@"   ""This is a quoted literal""")
            .ValidateTokens()
            .Count(1)
            .QuotedString(0, "This is a quoted literal")
            .Assert();
    }

    [Test]
    public void TestStringLiteral()
    {
        ServiceProvider
            .Tokenize(@"   ThisIsAStringLiteral")
            .ValidateTokens()
            .Count(1)
            .StringLiteral(0, "ThisIsAStringLiteral")
            .Assert();
    }
}