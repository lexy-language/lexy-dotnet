using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class MemberAccessExpression : Expression, IUsesTable
    {
        public MemberAccessLiteral MemberAccessLiteral { get; }

        public string Value { get; }

        public string Table { get; private set; }

        private MemberAccessExpression(MemberAccessLiteral literal, ExpressionSource source, SourceReference reference) : base(source, reference)
        {
            MemberAccessLiteral = literal ?? throw new ArgumentNullException(nameof(literal));
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
            if (MemberAccessLiteral.Parts.Length != 2)
            {
                context.Logger.Fail(Reference, "Invalid member access. Only 2 levels supported.");
                return;
            }
            var typeName = MemberAccessLiteral.Parent;
            if (!(context.Nodes.GetType(typeName) is ITypeWithMembers typeWithMembers))
            {
                context.Logger.Fail(Reference, $"Invalid member access. Type '{typeName}' not found.");
                return;
            }

            var memberName = MemberAccessLiteral.Member;
            var memberType = typeWithMembers.MemberType(memberName);
            if (memberType == null)
            {
                context.Logger.Fail(Reference, $"Invalid member access. Member '{memberName}' not found on '{typeName}'.");
            }

            if (typeWithMembers is TableType tableType)
            {
                Table = tableType.Type;
            }
        }

        public override VariableType DeriveType(IValidationContext context) => MemberAccessLiteral.DeriveType(context);
    }
}