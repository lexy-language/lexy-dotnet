using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public abstract class Token : IToken
{
    public abstract string Value { get; }
    public TokenCharacter FirstCharacter { get; }

    protected Token(TokenCharacter firstCharacter)
    {
        FirstCharacter = Assert.NotNull(firstCharacter, nameof(firstCharacter));
    }
}