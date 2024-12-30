using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Tokenizer;

public class NumberLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void IntLiteral()
    {
        ServiceProvider
            .Tokenize(@"   0")
            .ValidateTokens()
            .Count(1)
            .NumberLiteral(0, 0)
            .Assert();
    }

    [Test]
    public void Int3CharLiteral()
    {
        ServiceProvider
            .Tokenize(@"   456")
            .ValidateTokens()
            .Count(1)
            .NumberLiteral(0, 456)
            .Assert();
    }


    [Test]
    public void NegativeIntLiteral()
    {
        ServiceProvider
            .Tokenize(@"   -456")
            .ValidateTokens()
            .Count(2)
            .Operator(0, OperatorType.Subtraction)
            .NumberLiteral(1, 456)
            .Assert();
    }

    [Test]
    public void DecimalLiteral()
    {
        ServiceProvider
            .Tokenize(@"   456.78")
            .ValidateTokens()
            .Count(1)
            .NumberLiteral(0, 456.78m)
            .Assert();
    }

    [Test]
    public void NegativeDecimalLiteral()
    {
        ServiceProvider
            .Tokenize(@"   -456.78")
            .ValidateTokens()
            .Count(2)
            .Operator(0, OperatorType.Subtraction)
            .NumberLiteral(1, 456.78m)
            .Assert();
    }

    [Test]
    public void InvalidDecimalSubtract()
    {
        ServiceProvider
            .Tokenize(@"   456-78")
            .ValidateTokens()
            .Count(3)
            .NumberLiteral(0, 456)
            .Operator(1, OperatorType.Subtraction)
            .NumberLiteral(2, 78m)
            .Assert();
    }

    [Test]
    public void InvalidDecimalLiteral()
    {
        ServiceProvider
            .TokenizeExpectError(@"   456d78")
            .ErrorMessage.ShouldContain("Invalid number token character: 'd'");
    }

    [Test]
    public void InvalidDecimalOpenParLiteral()
    {
        ServiceProvider
            .TokenizeExpectError(@"   456(78")
            .ErrorMessage.ShouldContain("Invalid number token character: '('");
    }
}