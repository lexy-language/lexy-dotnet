using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions
{
    public class MemberAccessExpression : Expression, IHasNodeDependencies
    {
        public MemberAccessLiteral MemberAccessLiteral { get; }

        public string Value { get; }

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
            var variableType = context.FunctionCodeContext.GetVariableType(MemberAccessLiteral.Parent)
                            ?? context.Nodes.GetType(typeName);

            if (!(variableType is ITypeWithMembers typeWithMembers))
            {
                context.Logger.Fail(Reference, $"Invalid member access. Type '{typeName}' not found.");
                return;
            }

            var memberName = MemberAccessLiteral.Member;
            var memberType = typeWithMembers.MemberType(memberName, context);
            if (memberType == null)
            {
                context.Logger.Fail(Reference, $"Invalid member access. Member '{memberName}' not found on '{typeName}'.");
            }
        }

        public override VariableType DeriveType(IValidationContext context) => MemberAccessLiteral.DeriveType(context);

        public IEnumerable<IRootNode> GetNodes(Nodes nodes)
        {
            var rootNode = nodes.GetNode(MemberAccessLiteral.Parent);
            if (rootNode != null)
            {
                yield return rootNode;
            }
        }
    }
}