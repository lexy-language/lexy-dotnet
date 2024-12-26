using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser.Tokens
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
            var typeName = parts[0];
            if (!(context.Nodes.GetType(typeName) is ITypeWithMembers typeWithMembers))
            {
                return null;
            }

            var memberName = parts[1];
            return typeWithMembers.MemberType(memberName);
        }
    }
}