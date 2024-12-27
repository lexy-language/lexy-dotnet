using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp
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
                    return DefaultExpression(typeSyntax);

                case TypeNames.String:
                    return LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(""));

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
            return variableType switch
            {
                PrimitiveType primitive => Syntax(primitive.Type),
                EnumType enumType => IdentifierName(enumType.Type),
                TableType tableType => IdentifierName(tableType.Type),
                ComplexType complexType => ComplexTypeSyntax(complexType),
                ComplexTypeReference complexTypeReference => ComplexTypeReferenceSyntax(complexTypeReference),
                _ => throw new InvalidOperationException("Couldn't map type: " + variableType)
            };
        }

        private static TypeSyntax ComplexTypeReferenceSyntax(ComplexTypeReference complexTypeReference)
        {
            return complexTypeReference switch
            {
                FunctionParametersType _ => QualifiedName(
                    IdentifierName(ClassNames.FunctionClassName(complexTypeReference.Name)),
                    IdentifierName(LexyCodeConstants.ParametersType)),
                FunctionResultsType _ => QualifiedName(
                    IdentifierName(ClassNames.FunctionClassName(complexTypeReference.Name)),
                    IdentifierName(LexyCodeConstants.ResultsType)),
                TableRowType _ => QualifiedName(
                    IdentifierName(ClassNames.TableClassName(complexTypeReference.Name)),
                    IdentifierName(LexyCodeConstants.RowType)),
                _ => throw new InvalidOperationException($"Invalid type: {complexTypeReference?.GetType()}")
            };
        }

        private static TypeSyntax ComplexTypeSyntax(ComplexType complexType)
        {
            switch (complexType.Source)
            {
                case ComplexTypeSource.FunctionParameters:
                    return QualifiedName(
                        IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                        IdentifierName(LexyCodeConstants.ParametersType));
                case ComplexTypeSource.FunctionResults:
                    return QualifiedName(
                        IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                        IdentifierName(LexyCodeConstants.ResultsType));
                case ComplexTypeSource.TableRow:
                    return QualifiedName(
                        IdentifierName(ClassNames.TableClassName(complexType.Name)),
                        IdentifierName(LexyCodeConstants.RowType));
                case ComplexTypeSource.Custom:
                    return IdentifierName(ClassNames.CustomClassName(complexType.Name));
                default:
                    throw new InvalidOperationException($"Invalid type: {complexType}");
            }
        }

        public static TypeSyntax Syntax(VariableDeclarationType type)
        {
            return type switch
            {
                PrimitiveVariableDeclarationType primitive => Syntax(primitive.Type),
                CustomVariableDeclarationType enumType => IdentifierName(enumType.Type),
                ImplicitVariableDeclaration implicitVariable => Syntax(implicitVariable.VariableType),
                _ => throw new InvalidOperationException("Couldn't map type: " + type)
            };
        }
    }
}