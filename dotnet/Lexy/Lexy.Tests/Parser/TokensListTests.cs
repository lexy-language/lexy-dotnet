using Lexy.Compiler.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class TokensListTests
{
    [Test]
    public void TokensFrom()
    {
        var list = new TokenList(new[]
        {
            TokenFactory.String("123"),
            TokenFactory.String("456"),
            TokenFactory.String("789")
        });

        var result = list.TokensFrom(1);
        result.Length.ShouldBe(2);
        result[0].ValidateStringLiteralToken("456");
        result[1].ValidateStringLiteralToken("789");
    }

    [Test]
    public void TokensStart()
    {
        var list = new TokenList(new[]
        {
            TokenFactory.String("123"),
            TokenFactory.String("456"),
            TokenFactory.String("789")
        });

        var result = list.TokensFromStart(2);
        result.Length.ShouldBe(2);
        result[0].ValidateStringLiteralToken("123");
        result[1].ValidateStringLiteralToken("456");
    }

    [Test]
    public void TokensRange()
    {
        var list = new TokenList(new[]
        {
            TokenFactory.String("1111"),
            TokenFactory.String("2222"),
            TokenFactory.String("3333"),
            TokenFactory.String("4444"),
            TokenFactory.String("5555")
        });

        var result = list.TokensRange(1, 3);
        result.Length.ShouldBe(3);
        result[0].ValidateStringLiteralToken("2222");
        result[1].ValidateStringLiteralToken("3333");
        result[2].ValidateStringLiteralToken("4444");
    }
}