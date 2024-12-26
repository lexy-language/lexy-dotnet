using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Poc.Core.Language
{
    public class EnumMember : Node
    {
        public NumberLiteralToken Value { get; }
        public string Name { get; }

        private EnumMember(string name, SourceReference reference, NumberLiteralToken value = null) : base(reference)
        {
            Value = value;
            Name = name;
        }

        public static EnumMember Parse(IParserContext context)
        {
            var valid = context.ValidateTokens<AssignmentDefinition>()
                .CountMinimum(1)
                .StringLiteral(0)
                .IsValid;

            if (!valid) return null;

            var tokens = context.CurrentLine.Tokens;
            var name = tokens.TokenValue(0);
            var reference = context.LineStartReference();

            if (tokens.Length == 1)
            {
                return new EnumMember(name, reference);
            }

            if (tokens.Length != 3)
            {
                context.Logger.Fail(context.LineEndReference(),
                    $"Invalid number of tokens: {tokens.Length}. Should be 1 or 3.");
                return null;
            }

            valid = context.ValidateTokens<AssignmentDefinition>()
                .Operator(1, OperatorType.Assignment)
                .NumberLiteral(2)
                .IsValid;
            if (!valid) return null;

            var value = tokens.Token<NumberLiteralToken>(2);

            return new EnumMember(name, reference, value);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            ValidateMemberName(context);
            ValidateMemberValues(context);
        }

        private void ValidateMemberName(IValidationContext context)
        {
            if (string.IsNullOrEmpty(Name))
            {
                context.Logger.Fail(Reference, "Enum member name should not be null or empty.");
            }
            else if (!SyntaxFacts.IsValidIdentifier(Name))
            {
                context.Logger.Fail(Reference, $"Invalid enum member name: {Name}.");
            }
        }

        private void ValidateMemberValues(IValidationContext context)
        {
            if (Value == null) return;

            if (Value.NumberValue < 0)
            {
                context.Logger.Fail(Reference, $"Enum member value should not be < 0: {Value}");
            }

            if (Value.IsDecimal())
            {
                context.Logger.Fail(Reference, $"Enum member value should not be decimal: {Value}");
            }
        }
    }
}