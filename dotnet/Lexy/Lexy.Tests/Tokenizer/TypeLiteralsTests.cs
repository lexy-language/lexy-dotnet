using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class TypeLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestIntTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   int Value")
            .ValidateTokens()
            .Count(2)
            .StringLiteral(0, "int")
            .StringLiteral(1, "Value")
            .Assert();
    }

    [Test]
    public void TestNumberTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   number Value")
            .ValidateTokens()
            .Count(2)
            .StringLiteral(0, "number")
            .StringLiteral(1, "Value")
            .Assert();
    }

    [Test]
    public void TestStringTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   string Value")
            .ValidateTokens()
            .Count(2)
            .StringLiteral(0, "string")
            .StringLiteral(1, "Value")
            .Assert();
    }

    [Test]
    public void TestDateTimeTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   date Value")
            .ValidateTokens()
            .Count(2)
            .StringLiteral(0, "date")
            .StringLiteral(1, "Value")
            .Assert();
    }

    [Test]
    public void TestBooleanTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   boolean Value")
            .ValidateTokens()
            .Count(2)
            .StringLiteral(0, "boolean")
            .StringLiteral(1, "Value")
            .Assert();
    }
}