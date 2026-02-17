using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public abstract class Token : IToken
{
    private readonly int? endColumn;

    public abstract string Value { get; }

    public TokenCharacter FirstCharacter { get; }

    public int EndColumn => endColumn ?? FirstCharacter.Position + (Value != null ? Value.Length - 1 : 0);

    protected Token(TokenCharacter firstCharacter, int? endColumn = null)
    {
        this.endColumn = endColumn;
        FirstCharacter = Assert.NotNull(firstCharacter, nameof(firstCharacter));
    }
}
