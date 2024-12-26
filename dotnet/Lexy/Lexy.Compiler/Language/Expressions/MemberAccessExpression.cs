using System.Collections.Generic;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class MemberAccessExpression : Expression
    {
        private readonly MemberAccessLiteral literal;

        public string Value { get; }

        private MemberAccessExpression(MemberAccessLiteral literal, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            this.literal = literal;
            Value = literal.Value;
        }

        public static ParseExpressionResult Parse(ExpressionSource source)
        {
            var tokens = source.Tokens;
            if (!IsValid(tokens))
            {
                return ParseExpressionResult.Invalid<MemberAccessExpression>("Invalid expression.");
            }

            var literal = tokens.Token<MemberAccessLiteral>(0);
            var reference = source.CreateReference();

            var accessExpression = new MemberAccessExpression(literal, source, reference);
            return ParseExpressionResult.Success(accessExpression);
        }

        public static bool IsValid(TokenList tokens)
        {
            return tokens.Length == 1
                   && tokens.IsTokenType<MemberAccessLiteral>(0);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield break;
        }

        protected override void Validate(IValidationContext context)
        {
            var parts = literal.GetParts();
            if (parts.Length != 2)
            {
                context.Logger.Fail(Reference, "Invalid member access. Only 2 levels supported.");
                return;
            }
            var typeName = parts[0];
            var typeWithMembers = context.Nodes.GetType(typeName) as ITypeWithMembers;
            if (typeWithMembers == null)
            {
                context.Logger.Fail(Reference, $"Invalid member access. Type '{typeName}' not found.");
                return;
            }

            var memberName = parts[1];
            var memberType = typeWithMembers?.MemberType(memberName);
            if (memberType == null)
            {
                context.Logger.Fail(Reference, $"Invalid member access. Member '{memberName}' not found on '{typeName}'.");
            }
        }

        public override VariableType DeriveType(IValidationContext context)
        {
            return literal.DeriveType(context);
        }
    }
}