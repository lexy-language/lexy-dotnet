using System;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Parser.Tokens;

public class QuotedLiteralToken : ParsableToken, ILiteralToken
{
    private bool quoteClosed;

    public QuotedLiteralToken(TokenCharacter character) : base(null, character)
    {
        var value = character.Value;
        if (value != TokenValues.Quote)
        {
            throw new InvalidOperationException("QuotedLiteralToken should start with a quote");
        }
    }

    public object TypedValue => Value;

    public VariableType DeriveType(IValidationContext context)
    {
        return PrimitiveType.String;
    }

    public override ParseTokenResult Parse(TokenCharacter character)
    {
        var value = character.Value;
        if (quoteClosed) throw new InvalidOperationException("No characters allowed after closing quote.");

        if (value == TokenValues.Quote)
        {
            quoteClosed = true;
            return ParseTokenResult.Finished(true);
        }

        AppendValue(value);
        return ParseTokenResult.InProgress();
    }

    public override ParseTokenResult EndOfLine()
    {
        if (!quoteClosed) return ParseTokenResult.Invalid("Closing quote expected.");
        throw new InvalidOperationException("Token should be finished by the Parse method before reaching end of line");
    }

    public override string ToString()
    {
        return Value;
    }
}