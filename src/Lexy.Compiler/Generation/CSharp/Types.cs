using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

namespace Lexy.Compiler.Generation.CSharp;

internal static class Types
{
    public static ExpressionSyntax DateSyntax(DateTimeLiteralToken dateTimeLiteralToken)
    {
        return DateSyntax(dateTimeLiteralToken.DateTimeValue);
    }

    private static ExpressionSyntax DateSyntax(DateTime dateTimeValue)
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
        return Syntax(variableDefinition.TypeDeclaration);
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

    public static TypeSyntax Syntax(Type type)
    {
        return type switch
        {
            ValueType value => Syntax(value.Name),
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Name)),
            TableType tableType => IdentifierName(tableType.Name),
            DeclaredType declaredType => DeclaredTypeSyntax(declaredType),
            GeneratedType generatedType => ObjectTypeSyntax(generatedType),
            _ => throw new InvalidOperationException($"Not supported: {type.GetType()}")
        };
    }

    private static TypeSyntax DeclaredTypeSyntax(DeclaredType generatedType)
    {
        return IdentifierName(ClassNames.TypeClassName(generatedType.Name));
    }

    private static TypeSyntax ObjectTypeSyntax(GeneratedType generatedType)
    {
        var functionClassName = generatedType.Source switch
        {
            GeneratedTypeSource.FunctionParameters => ClassNames.FunctionClassName(generatedType.TypeName),
            GeneratedTypeSource.FunctionResults => ClassNames.FunctionClassName(generatedType.TypeName),
            GeneratedTypeSource.TableRow => ClassNames.TableClassName(generatedType.TypeName),
            _ => throw new InvalidOperationException($"Invalid type: {generatedType}")
        };

        var memberName = generatedType.Source switch
        {
            GeneratedTypeSource.FunctionParameters => LexyCodeConstants.ParametersType,
            GeneratedTypeSource.FunctionResults => LexyCodeConstants.ResultsType,
            GeneratedTypeSource.TableRow => LexyCodeConstants.RowType,
            _ => throw new InvalidOperationException($"Invalid type: {generatedType}")
        };

        return QualifiedName(
            IdentifierName(functionClassName),
            IdentifierName(memberName));
    }

    public static TypeSyntax Syntax(TypeDeclaration typeDeclaration)
    {
        return typeDeclaration switch
        {
            ValueTypeDeclaration value => Syntax(value.Type),
            ObjectTypeDeclaration objectDeclaration => IdentifierNameSyntax(objectDeclaration),
            ImplicitTypeDeclaration implicitVariable => Syntax(implicitVariable.Type),
            _ => throw new InvalidOperationException("Couldn't map type: " + typeDeclaration)
        };
    }

    private static TypeSyntax IdentifierNameSyntax(ObjectTypeDeclaration objectType)
    {
        return objectType.Type switch
        {
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Name)),
            TableType tableType => IdentifierName(ClassNames.TableClassName(tableType.Name)),
            DeclaredType declaredType => IdentifierName(ClassNames.TypeClassName(declaredType.Name)),
            GeneratedType generatedType => ObjectTypeSyntax(generatedType),
            _ => throw new InvalidOperationException($"Couldn't map type: {objectType.Type}")
        };
    }

    public static ExpressionSyntax TypeDefaultExpression(TypeDeclaration typeDeclaration)
    {
        return typeDeclaration switch
        {
            ValueTypeDeclaration expression => ValueTypeDefaultExpression(expression),
            ObjectTypeDeclaration declaredType => DefaultExpressionSyntax(declaredType),
            _ => throw new InvalidOperationException(
                $"Wrong VariableDeclarationType {typeDeclaration.GetType()}")
        };
    }

    private static ExpressionSyntax DefaultExpressionSyntax(ObjectTypeDeclaration objectType)
    {
        if (objectType.Type is DeclaredType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(objectType)).WithArgumentList(ArgumentList());
        }

        if (objectType.Type is GeneratedType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(objectType)).WithArgumentList(ArgumentList());
        }
        return DefaultExpression(IdentifierNameSyntax(objectType));
    }

    private static ExpressionSyntax ValueTypeDefaultExpression(ValueTypeDeclaration type)
    {
        switch (type.TypeName)
        {
            case TypeNames.Number:
            case TypeNames.Boolean:
                var typeSyntax = Syntax(type);
                return DefaultExpression(typeSyntax);

            case TypeNames.String:
                return LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(string.Empty));

            case TypeNames.Date:
                return DateSyntax(DateTypeDefault.Value);

            default:
                throw new InvalidOperationException("Invalid type: " + type.Type);
        }
    }
}
