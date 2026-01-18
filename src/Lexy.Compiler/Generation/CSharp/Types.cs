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
            ValueType primitive => Syntax(primitive.Type),
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
        return generatedType.Source switch
        {
            GeneratedTypeSource.FunctionParameters => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.ParametersType)),
            GeneratedTypeSource.FunctionResults => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.ResultsType)),
            GeneratedTypeSource.TableRow => QualifiedName(IdentifierName(ClassNames.TableClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.RowType)),
            _ => throw new InvalidOperationException($"Invalid type: {generatedType}")
        };
    }

    public static TypeSyntax Syntax(TypeDeclaration typeDeclaration)
    {
        return typeDeclaration switch
        {
            PrimitiveTypeDeclaration primitive => Syntax(primitive.Type),
            ObjectTypeDeclaration objectDeclararion => IdentifierNameSyntax(objectDeclararion),
            ImplicitTypeDeclaration implicitVariable => Syntax(implicitVariable.Type),
            _ => throw new InvalidOperationException("Couldn't map type: " + typeDeclaration)
        };
    }

    private static TypeSyntax IdentifierNameSyntax(ObjectTypeDeclaration objectType)
    {
        return ((TypeDeclaration)objectType).Type switch
        {
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Name)),
            TableType tableType => IdentifierName(ClassNames.TableClassName(tableType.Name)),
            DeclaredType declaredType => IdentifierName(ClassNames.TypeClassName(declaredType.Name)),
            GeneratedType generatedType => ObjectTypeIdentifierNameSyntax(generatedType),
            _ => throw new InvalidOperationException("Couldn't map type: " + ((TypeDeclaration)objectType).Type)
        };
    }

    private static TypeSyntax ObjectTypeIdentifierNameSyntax(GeneratedType generatedType)
    {
        return generatedType.Source switch
        {
            GeneratedTypeSource.FunctionParameters => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.ParametersType)),
            GeneratedTypeSource.FunctionResults => QualifiedName(
                IdentifierName(ClassNames.FunctionClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.ResultsType)),
            GeneratedTypeSource.TableRow => QualifiedName(
                IdentifierName(ClassNames.TableClassName(generatedType.Name)),
                IdentifierName(LexyCodeConstants.RowType)),
            _ => throw new InvalidOperationException("Invalid GeneratedType source: " + generatedType.Source)
        };
    }

    public static ExpressionSyntax TypeDefaultExpression(TypeDeclaration typeDeclaration)
    {
        return typeDeclaration switch
        {
            PrimitiveTypeDeclaration expression => PrimitiveTypeDefaultExpression(expression),
            ObjectTypeDeclaration declaredType => DefaultExpressionSyntax(declaredType),
            _ => throw new InvalidOperationException(
                $"Wrong VariableDeclarationType {typeDeclaration.GetType()}")
        };
    }

    private static ExpressionSyntax DefaultExpressionSyntax(ObjectTypeDeclaration @object)
    {
        if (((TypeDeclaration)@object).Type is DeclaredType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(@object)).WithArgumentList(ArgumentList());
        }

        if (((TypeDeclaration)@object).Type is GeneratedType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(@object)).WithArgumentList(ArgumentList());
        }
        return DefaultExpression(IdentifierNameSyntax(@object));
    }

    private static ExpressionSyntax PrimitiveTypeDefaultExpression(PrimitiveTypeDeclaration type)
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
                    Literal(""));

            case TypeNames.Date:
                return DateSyntax(DateTypeDefault.Value);

            default:
                throw new InvalidOperationException("Invalid type: " + type.Type);
        }
    }
}
