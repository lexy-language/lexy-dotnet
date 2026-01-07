using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            DeclaredType declaredType => DeclaredTypeSyntax(declaredType),
            GeneratedType generatedType => ObjectTypeSyntax(generatedType),
            _ => throw new InvalidOperationException($"Not supported: {variableType.GetType()}")
        };
    }

    private static TypeSyntax DeclaredTypeSyntax(DeclaredType generatedType)
    {
        return IdentifierName(ClassNames.TypeClassName(generatedType.Type));
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

    public static TypeSyntax Syntax(VariableTypeDeclaration type)
    {
        return type switch
        {
            PrimitiveVariableTypeDeclaration primitive => Syntax(primitive.Type),
            ObjectVariableTypeDeclaration objectDeclararion => IdentifierNameSyntax(objectDeclararion),
            ImplicitVariableTypeDeclaration implicitVariable => Syntax(implicitVariable.VariableType),
            _ => throw new InvalidOperationException("Couldn't map type: " + type)
        };
    }

    private static TypeSyntax IdentifierNameSyntax(ObjectVariableTypeDeclaration objectVariableType)
    {
        return objectVariableType.VariableType switch
        {
            EnumType enumType => IdentifierName(ClassNames.EnumClassName(enumType.Type)),
            TableType tableType => IdentifierName(ClassNames.TableClassName(tableType.TableName)),
            DeclaredType declaredType => IdentifierName(ClassNames.TypeClassName(declaredType.Type)),
            GeneratedType generatedType => ObjectTypeIdentifierNameSyntax(generatedType),
            _ => throw new InvalidOperationException("Couldn't map type: " + objectVariableType.VariableType)
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

    public static ExpressionSyntax TypeDefaultExpression(VariableTypeDeclaration variableTypeDeclaration)
    {
        return variableTypeDeclaration switch
        {
            PrimitiveVariableTypeDeclaration expression => PrimitiveTypeDefaultExpression(expression),
            ObjectVariableTypeDeclaration declaredType => DefaultExpressionSyntax(declaredType),
            _ => throw new InvalidOperationException(
                $"Wrong VariableDeclarationType {variableTypeDeclaration.GetType()}")
        };
    }

    private static ExpressionSyntax DefaultExpressionSyntax(ObjectVariableTypeDeclaration @object)
    {
        if (@object.VariableType is DeclaredType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(@object)).WithArgumentList(ArgumentList());
        }

        if (@object.VariableType is GeneratedType)
        {
            return ObjectCreationExpression(IdentifierNameSyntax(@object)).WithArgumentList(ArgumentList());
        }
        return DefaultExpression(IdentifierNameSyntax(@object));
    }

    private static ExpressionSyntax PrimitiveTypeDefaultExpression(PrimitiveVariableTypeDeclaration type)
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
                return DateSyntax(DateTypeDefault.Value);

            default:
                throw new InvalidOperationException("Invalid type: " + type.Type);
        }
    }
}