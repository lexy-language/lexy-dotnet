using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public class BooleanLiteral : Token, ILiteralToken
    {
        public bool BooleanValue { get; }

        public override string Value => BooleanValue ? TokenValues.BooleanTrue : TokenValues.BooleanFalse;

        public BooleanLiteral(bool value, TokenCharacter character) : base(character)
        {
            BooleanValue = value;
        }

        public static BooleanLiteral Parse(string value, TokenCharacter character)
        {
            return value switch
            {
                TokenValues.BooleanTrue => new BooleanLiteral(true, character),
                TokenValues.BooleanFalse => new BooleanLiteral(false, character),
                _ => throw new InvalidOperationException($"Couldn't parse boolean: {value}")
            };
        }

        public static bool IsValid(string value)
        {
            return value == TokenValues.BooleanTrue || value == TokenValues.BooleanFalse;
        }

        public override string ToString() => Value;

        public VariableType DeriveType(IValidationContext context) => PrimitiveType.Boolean;
    }
}