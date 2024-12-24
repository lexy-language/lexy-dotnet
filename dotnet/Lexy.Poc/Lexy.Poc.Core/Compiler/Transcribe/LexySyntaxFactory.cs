using System;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Poc.Core.Compiler.Transcribe
{
    public static class LexySyntaxFactory
    {
        public static ExpressionSyntax TokenValueExpression(ILiteralToken token)
        {
            if (token == null) return null;

            return token switch
            {
                QuotedLiteralToken _ => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(token.Value)),
                NumberLiteralToken number => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal($"{number.NumberValue}m", number.NumberValue)),
                DateTimeLiteral dateTimeLiteral => TranslateDateTime(dateTimeLiteral),
                BooleanLiteral boolean => LiteralExpression(boolean.BooleanValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression),
                MemberAccessLiteral memberAccess => TranslateMemberAccessLiteral(memberAccess),
                _ => throw new InvalidOperationException("Couldn't map type: " + token.GetType())
            };
        }

        private static ExpressionSyntax TranslateMemberAccessLiteral(MemberAccessLiteral memberAccess)
        {
            var parts = memberAccess.GetParts();
            if (parts.Length != 2) throw new InvalidOperationException("Only 2 parts expected.");

            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(parts[0]),
                IdentifierName(parts[1]));
        }

        private static ExpressionSyntax TranslateDateTime(DateTimeLiteral dateTimeLiteral)
        {
            return ObjectCreationExpression(
                QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("DateTime")))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Year),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Month),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Day),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Hour),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Minute),
                                Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeLiteral.DateTimeValue.Second)
                            })));
        }

        public static TypeSyntax MapType(VariableDefinition variableDefinition) => MapType(variableDefinition.Type);

        public static TypeSyntax MapType(string type)
        {
            return type switch
            {
                TypeNames.String => PredefinedType(Token(SyntaxKind.StringKeyword)),
                TypeNames.Number => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
                TypeNames.DateTime => ParseName("System.DateTime"),
                TypeNames.Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }

        public static TypeSyntax MapType(VariableDeclarationType type)
        {
            return type switch
            {
                PrimitiveVariableDeclarationType primitive => MapType(primitive.Type),
                CustomVariableDeclarationType enumType => IdentifierName(enumType.Type),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }
    }
}