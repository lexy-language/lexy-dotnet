using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Tests.Parser;

public static class TokenFactory
{
    public static StringLiteralToken String(string value)
    {
        return new StringLiteralToken(value, TestTokenCharacter.Dummy);
    }
}