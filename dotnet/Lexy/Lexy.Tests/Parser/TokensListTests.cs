using Lexy.Poc.Core;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Lexy.Poc.Parser.ExpressionParser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class TokensListTests
    {
        [Test]
        public void TokensFrom()
        {
            var list = new TokenList(new []
            {
                TokenFactory.String("123"),
                TokenFactory.String("456"),
                TokenFactory.String("789"),
            });

            var result = list.TokensFrom(1);
            result.Length.ShouldBe(2);
            result[0].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("456"));
            result[1].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("789"));
        }

        [Test]
        public void TokensStart()
        {
            var list = new TokenList(new []
            {
                TokenFactory.String("123"),
                TokenFactory.String("456"),
                TokenFactory.String("789"),
            });

            var result = list.TokensFromStart(2);
            result.Length.ShouldBe(2);
            result[0].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("123"));
            result[1].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("456"));
        }

        [Test]
        public void TokensRange()
        {
            var list = new TokenList(new []
            {
                TokenFactory.String("1111"),
                TokenFactory.String("2222"),
                TokenFactory.String("3333"),
                TokenFactory.String("4444"),
                TokenFactory.String("5555"),
            });

            var result = list.TokensRange(1, 3);
            result.Length.ShouldBe(3);
            result[0].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("2222"));
            result[1].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("3333"));
            result[2].ValidateOfType<StringLiteralToken>(value => value.Value.ShouldBe("4444"));
        }
    }

    public static class TokenFactory
    {
        public static StringLiteralToken String(string value)
        {
            return new StringLiteralToken(value, TestTokenCharacter.Dummy);
        }
    }

    public class TestTokenCharacter
    {
        public static TokenCharacter Dummy => new TokenCharacter('a', 0);
    }
}