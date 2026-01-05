using System;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Parser.Tokens;

public class BooleanLiteralToken : Token, ILiteralToken
{
    public bool BooleanValue { get; }

    public BooleanLiteralToken(bool value, TokenCharacter character) : base(character)
    {
        BooleanValue = value;
    }

    public override string Value => BooleanValue ? TokenValues.BooleanTrue : TokenValues.BooleanFalse;

    public object TypedValue => BooleanValue;

    public VariableType DeriveType(IValidationContext context)
    {
        return PrimitiveType.Boolean;
    }

    public static BooleanLiteralToken Parse(string value, TokenCharacter character)
    {
        return value switch
        {
            TokenValues.BooleanTrue => new BooleanLiteralToken(true, character),
            TokenValues.BooleanFalse => new BooleanLiteralToken(false, character),
            _ => throw new InvalidOperationException($"Couldn't parse boolean: {value}")
        };
    }

    public static bool IsValid(string value)
    {
        return value == TokenValues.BooleanTrue || value == TokenValues.BooleanFalse;
    }

    public override string ToString()
    {
        return Value;
    }
}