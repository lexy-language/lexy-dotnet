using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser.Tokens
{
    public class MemberAccessLiteral : Token, ILiteralToken
    {
        private readonly string[] parts;
        public override string Value { get; }

        public VariableDeclarationType Type => GetEnumType();

        public string Parent => parts.Length >= 1 ? parts[0] : null;
        public string Member => parts.Length >= 2 ? parts[1] : null;

        public string[] Parts => parts;

        public MemberAccessLiteral(string value, TokenCharacter character) : base(character)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            parts = Value.Split(TokenValues.MemberAccess);
        }

        public override string ToString() => Value;

        private VariableDeclarationType GetEnumType()
        {
            return parts.Length == 2 ? new CustomVariableDeclarationType(parts[0]) : null;
        }

        public VariableType DeriveType(IValidationContext context)
        {
            if (parts.Length != 2)
            {
                return null;
            }

            var variableType = context.FunctionCodeContext.GetVariableType(Parent)
                               ?? context.Nodes.GetType(Parent);
            if (!(variableType is ITypeWithMembers typeWithMembers))
            {
                return null;
            }

            return typeWithMembers.MemberType(Member, context);
        }
    }
}