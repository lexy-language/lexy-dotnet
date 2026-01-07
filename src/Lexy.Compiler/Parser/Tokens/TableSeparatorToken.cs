namespace Lexy.Compiler.Parser.Tokens;

public class TableSeparatorToken : ParsableToken
{
    public TableSeparatorToken(TokenCharacter character) : base(character)
    {
    }

    public override ParseTokenResult Parse(TokenCharacter character)
    {
        return ParseTokenResult.Finished(true);
    }

    public override ParseTokenResult EndOfLine()
    {
        return ParseTokenResult.Finished(true);
    }
}