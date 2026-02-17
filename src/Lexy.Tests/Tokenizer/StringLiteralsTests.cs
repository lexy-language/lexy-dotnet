using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Tokenizer;

public class StringLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestQuotedLiteral()
    {
        ServiceProvider
            .Tokenize(@"   ""This is a quoted literal""")
            .Count(1)
            .QuotedString(0, "This is a quoted literal")
            .Assert();
    }

    [Test]
    public void TestQuotedLiteralWithEscapedQuote()
    {
        ServiceProvider
            .Tokenize(@"   ""This is \\a quoted \""literal\""""")
            .Count(1)
            .QuotedString(0, @"This is \a quoted ""literal""")
            .Assert();
    }

    [Test]
    public void TestStringLiteral()
    {
        ServiceProvider
            .Tokenize(@"   ThisIsAStringLiteral")
            .Count(1)
            .StringLiteral(0, "ThisIsAStringLiteral")
            .Assert();
    }

    [Test]
    public void TestOpenEndStringLiteral()
    {
        ServiceProvider
            .TokenizeExpectError(@"   ""ThisIsAStringLiteral")
            .ErrorMessage.ShouldBe("Invalid token at end of line. Closing quote expected.");
    }
}
