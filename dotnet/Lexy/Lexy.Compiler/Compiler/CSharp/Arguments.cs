using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal static class Arguments
    {
        public static SyntaxNode Numeric(int value)
        {
            return Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(value)));
        }

        public static SyntaxNode String(string value)
        {
            return Argument(
                LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(value)));
        }

        public static SyntaxNode MemberAccess(string value, string member)
        {
            return Argument(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(value),
                    IdentifierName(member)));
        }

        public static SyntaxNode MemberAccessLambda(string parameter, string member)
        {
            return Argument(SimpleLambdaExpression(Parameter(Identifier(parameter)))
                .WithExpressionBody(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(parameter),
                        IdentifierName(member))));
        }
    }
}