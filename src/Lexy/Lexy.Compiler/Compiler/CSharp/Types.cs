using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lexy.Compiler.Compiler.CSharp;

internal static class Types
{
    public static ExpressionSyntax TranslateDate(DateTimeLiteral dateTimeLiteral)
    {
        return TranslateDate(dateTimeLiteral.DateTimeValue);
    }

    private static ExpressionSyntax TranslateDate(DateTime dateTimeValue)
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
                            Arguments.Numeric(dateTimeValue.Year),
                            Token(SyntaxKind.CommaToken),
                            Arguments.Numeric(dateTimeValue.Month),
                            Token(SyntaxKind.CommaToken),
                            Arguments.Numeric(dateTimeValue.Day),
                            Token(SyntaxKind.CommaToken),
                            Arguments.Numeric(dateTimeValue.Hour),
                            Token(SyntaxKind.CommaToken),
                            Arguments.Numeric(dateTimeValue.Minute),
                            Token(SyntaxKind.CommaToken),
                            Arguments.Numeric(dateTimeValue.Second)
                        })));
    }

    public static TypeSyntax Syntax(VariableDefinition variableDefinition)
    {
        return Syntax(variableDefinition.Type);
    }

    public static TypeSyntax Syntax(string type)
    {
        return type switch
        {
            TypeNames.String => PredefinedType(Token(SyntaxKind.StringKeyword)),
            TypeNames.Number => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
            TypeNames.Date => ParseName("System.DateTime"),
            TypeNames.Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
            _ => throw new InvalidOperationException("Couldn't map type: " + type)
        };
    }

    public static TypeSyntax Syntax(VariableType variableType)
    {
        return variableType switch
        {
            PrimitiveType primitive => Syntax(primitive.Type),
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Type)),
            TableType tableType => IdentifierName(tableType.TableName),
            ComplexType complexType => ComplexTypeSyntax(complexType),
            _ => throw new InvalidOperationException("Couldn't map type: " + variableType)
        };
    }

    private static TypeSyntax ComplexTypeSyntax(ComplexType complexType)
    {
        return complexType.Source switch
        {
            ComplexTypeSource.FunctionParameters => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.ParametersType)),
            ComplexTypeSource.FunctionResults => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.ResultsType)),
            ComplexTypeSource.TableRow => QualifiedName(IdentifierName(ClassNames.TableClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.RowType)),
            _ => throw new InvalidOperationException($"Invalid type: {complexType}")
        };
    }

    public static TypeSyntax Syntax(VariableDeclarationType type)
    {
        return type switch
        {
            PrimitiveVariableDeclarationType primitive => Syntax(primitive.Type),
            CustomVariableDeclarationType customVariable => IdentifierNameSyntax(customVariable),
            ImplicitVariableDeclaration implicitVariable => Syntax(implicitVariable.VariableType),
            _ => throw new InvalidOperationException("Couldn't map type: " + type)
        };
    }

    private static TypeSyntax IdentifierNameSyntax(CustomVariableDeclarationType customVariable)
    {
        return customVariable.VariableType switch
        {
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Type)),
            TableType tableType => IdentifierName(ClassNames.TableClassName(tableType.TableName)),
            CustomType customType => IdentifierName(ClassNames.TypeClassName(customType.Type)),
            ComplexType complexType => ComplexTypeIdentifierNameSyntax(complexType),
            _ => throw new InvalidOperationException("Couldn't map type: " + customVariable.VariableType)
        };
    }

    private static TypeSyntax ComplexTypeIdentifierNameSyntax(ComplexType complexType)
    {
        return complexType.Source switch
        {
            ComplexTypeSource.FunctionParameters => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.ParametersType)),
            ComplexTypeSource.FunctionResults => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.ResultsType)),
            ComplexTypeSource.TableRow => QualifiedName(
                IdentifierName(ClassNames.TableClassName(complexType.Name)),
                IdentifierName(LexyCodeConstants.RowType)),
            _ => throw new InvalidOperationException("Invalid ComplexType source: " + complexType.Source)
        };
    }

    public static ExpressionSyntax TypeDefaultExpression(VariableDeclarationType variableDeclarationType)
    {
        return variableDeclarationType switch
        {
            PrimitiveVariableDeclarationType expression => PrimitiveTypeDefaultExpression(expression),
            CustomVariableDeclarationType customType => DefaultExpressionSyntax(customType),
            _ => throw new InvalidOperationException(
                $"Wrong VariableDeclarationType {variableDeclarationType.GetType()}")
        };
    }

    private static ExpressionSyntax DefaultExpressionSyntax(CustomVariableDeclarationType customType)
    {
        if (customType.VariableType is CustomType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(customType)).WithArgumentList(ArgumentList());
        }

        if (customType.VariableType is ComplexType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(customType)).WithArgumentList(ArgumentList());
        }
        return DefaultExpression(IdentifierNameSyntax(customType));
    }

    private static ExpressionSyntax PrimitiveTypeDefaultExpression(PrimitiveVariableDeclarationType type)
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
}