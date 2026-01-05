using System;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.Syntax;

internal static class TokenValuesSyntax
{
    public static ExpressionSyntax Expression(ILiteralToken token)
    {
        if (token == null) return null;

        return token switch
        {
            QuotedLiteralToken _ => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(token.Value)),
            NumberLiteralToken number => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal($"{number.NumberValue}m", number.NumberValue)),
            DateTimeLiteralToken dateTimeLiteral => Types.DateSyntax(dateTimeLiteral),
            BooleanLiteralToken boolean => SyntaxFactory.LiteralExpression(boolean.BooleanValue
                ? SyntaxKind.TrueLiteralExpression
                : SyntaxKind.FalseLiteralExpression),
            MemberAccessLiteralToken memberAccess => MemberAccessLiteralSyntax(memberAccess),
            _ => throw new InvalidOperationException("Couldn't map type: " + token.GetType())
        };
    }

    private static ExpressionSyntax MemberAccessLiteralSyntax(MemberAccessLiteralToken memberAccess)
    {
        var parts = memberAccess.Parts;
        if (parts.Length != 2) throw new InvalidOperationException("Only 2 parts expected.");

        var identifierNameSyntax = SyntaxFactory.IdentifierName(parts[0]);

        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            identifierNameSyntax,
            SyntaxFactory.IdentifierName(parts[1]));
    }
}