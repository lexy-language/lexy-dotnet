using System;
using System.Text;

namespace Lexy.Compiler.Parser.Tokens;

public abstract class ParsableToken : Token
{
    private readonly StringBuilder valueBuilder;

    public int ValueLength => valueBuilder.Length;

    public override string Value => valueBuilder.ToString();

    protected ParsableToken(TokenCharacter character) : base(character)
    {
        if (character == null) throw new ArgumentNullException(nameof(character));
        valueBuilder = new StringBuilder(character.Value.ToString());
    }

    protected ParsableToken(string value, TokenCharacter position) : base(position)
    {
        valueBuilder = new StringBuilder(value);
    }

    protected void AppendValue(char value)
    {
        valueBuilder.Append(value);
    }

    protected void AppendValue(string value)
    {
        valueBuilder.Append(value);
    }

    public abstract ParseTokenResult Parse(TokenCharacter character);

    public abstract ParseTokenResult Finalize();
}