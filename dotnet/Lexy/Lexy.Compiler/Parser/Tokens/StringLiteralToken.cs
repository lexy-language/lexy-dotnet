using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser.Tokens
{
    public class StringLiteralToken : Token, ILiteralToken
    {
        public override string Value { get; }

        public object TypedValue => Value;

        public StringLiteralToken(string value, TokenCharacter character) : base(character)
        {
            Value = value;
        }

        public override string ToString() => Value;

        public VariableType DeriveType(IValidationContext context) => throw new InvalidOperationException("Not supported. Type should be defined by node or expression.");
    }
}