using System;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal static class Types
    {
        public static ExpressionSyntax PrimitiveTypeDefaultExpression(PrimitiveVariableDeclarationType type)
        {
            switch (type.Type)
            {
                case TypeNames.Number:
                case TypeNames.Boolean:
                    var typeSyntax = Syntax(type);
                    return SyntaxFactory.DefaultExpression(typeSyntax);

                case TypeNames.String:
                    return SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(""));

                case TypeNames.Date:
                    return TranslateDate(DateTypeDefault.Value);

                default:
                    throw new InvalidOperationException("Invalid type: " + type.Type);
            }
        }

        public static ExpressionSyntax TranslateDate(DateTimeLiteral dateTimeLiteral)
        {
            return TranslateDate(dateTimeLiteral.DateTimeValue);
        }

        private static ExpressionSyntax TranslateDate(DateTime dateTimeValue)
        {
            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("System"),
                        SyntaxFactory.IdentifierName("DateTime")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Arguments.Numeric(dateTimeValue.Year),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Month),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Day),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Hour),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Minute),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                Arguments.Numeric(dateTimeValue.Second)
                            })));
        }

        public static TypeSyntax Syntax(VariableDefinition variableDefinition) => Syntax(variableDefinition.Type);

        public static TypeSyntax Syntax(string type)
        {
            return type switch
            {
                TypeNames.String => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                TypeNames.Number => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword)),
                TypeNames.Date => SyntaxFactory.ParseName("System.DateTime"),
                TypeNames.Boolean => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }

        public static TypeSyntax Syntax(VariableType variableType)
        {
            if (variableType.Equals(PrimitiveType.String))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            }

            if (variableType.Equals(PrimitiveType.Number))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword));
            }

            if (variableType.Equals(PrimitiveType.Date))
            {
                return SyntaxFactory.ParseName("System.DateTime");
            }

            if (variableType.Equals(PrimitiveType.Boolean))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
            }

            throw new InvalidOperationException("Couldn't map type: " + variableType);
        }

        public static TypeSyntax Syntax(VariableDeclarationType type)
        {
            return type switch
            {
                PrimitiveVariableDeclarationType primitive => Syntax(primitive.Type),
                CustomVariableDeclarationType enumType => SyntaxFactory.IdentifierName(enumType.Type),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }
    }
}