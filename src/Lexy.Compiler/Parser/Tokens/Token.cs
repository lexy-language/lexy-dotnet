using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public abstract class Token : IToken
{
    public abstract string Value { get; }

    public TokenCharacter FirstCharacter { get; }

    public int EndColumn => FirstCharacter.Position + (Value != null ? Value.Length - 1 : 0);

    protected Token(TokenCharacter firstCharacter)
    {
        FirstCharacter = Assert.NotNull(firstCharacter, nameof(firstCharacter));
    }
}
