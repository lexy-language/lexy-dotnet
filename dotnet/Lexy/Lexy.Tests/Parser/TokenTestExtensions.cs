using System;
using Lexy.Compiler.Parser.Tokens;
using Lexy.Tests.Parser.ExpressionParser;
using Shouldly;

namespace Lexy.Tests.Parser;

public static class TokenTestExtensions
{
    public static void ValidateStringLiteralToken(this Token token, string value)
    {
        if (token == null) throw new ArgumentNullException(nameof(token));
        token.ValidateOfType<StringLiteralToken>(actual => ShouldBeStringTestExtensions.ShouldBe(actual.Value, value));
    }
}