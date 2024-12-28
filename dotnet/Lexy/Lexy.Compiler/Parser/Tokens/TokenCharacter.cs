namespace Lexy.Compiler.Parser.Tokens;

public class TokenCharacter
{
    public int Position { get; }
    public char Value { get; }

    public TokenCharacter(char value, int position)
    {
        Value = value;
        Position = position;
    }
}