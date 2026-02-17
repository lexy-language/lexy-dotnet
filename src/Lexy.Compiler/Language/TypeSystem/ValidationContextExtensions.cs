using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem;

public static class ValidationContextExtensions
{
    public static void ValidateTypeAndDefault(this IValidationContext context, SourceReference reference,
        TypeDeclaration type, Expression defaultValueExpression)
    {
        Assert.NotNull(context, nameof(context));
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(type, nameof(type));

        switch (type)
        {
            case ObjectTypeDeclaration customType:
                ValidateCustomType(context, reference, customType, defaultValueExpression);
                break;

            case ValueTypeDeclaration valueType:
                ValidateValueType(context, reference, valueType, defaultValueExpression);
                break;

            default:
                throw new InvalidOperationException($"Invalid Type: {type.GetType()}");
        }
    }

    private static void ValidateCustomType(IValidationContext context, SourceReference reference,
        ObjectTypeDeclaration objectTypeDeclaration, Expression defaultValueExpression)
    {
        var identifierPathComplex = IdentifierPath.Parse(objectTypeDeclaration.TypeName);
        var variable = context.VariableContext.CreateVariableReference(reference, identifierPathComplex);
        var type = variable?.Type;
        if (type == null ||
            type is not EnumType
         && type is not DeclaredType
         && type is not GeneratedType)
        {
            //logged by CustomVariableDeclarationType
            return;
        }

        if (defaultValueExpression == null) return;

        if (type is not EnumType)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{objectTypeDeclaration.Type}') does not support a default value.");
            return;
        }

        if (defaultValueExpression is not MemberAccessExpression memberAccessExpression
         || memberAccessExpression.IdentifierPath == null)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{objectTypeDeclaration.Type}')");
            return;
        }

        var identifierPath = memberAccessExpression.IdentifierPath;
        if (identifierPath.Parts != 2)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{objectTypeDeclaration.Type}')");
        }
        if (identifierPath.RootIdentifier != objectTypeDeclaration.TypeName)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. Invalid enum type. (type: '{objectTypeDeclaration.Type}')");
        }

        var enumDeclaration = context.ComponentNodes.GetEnum(identifierPath.RootIdentifier);
        if (enumDeclaration == null || !enumDeclaration.ContainsMember(identifierPath.Path[1]))
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. Invalid member. (type: '{objectTypeDeclaration.Type}')");
        }
    }

    private static void ValidateValueType(IValidationContext context, SourceReference reference,
        ValueTypeDeclaration valueTypeDeclaration, Expression defaultValueExpression)
    {
        if (defaultValueExpression == null) return;

        switch (valueTypeDeclaration.TypeName)
        {
            case TypeNames.Number:
                ValidateDefaultLiteral<NumberLiteralToken>(context, reference, valueTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.String:
                ValidateDefaultLiteral<QuotedLiteralToken>(context, reference, valueTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.Boolean:
                ValidateDefaultLiteral<BooleanLiteralToken>(context, reference, valueTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.Date:
                ValidateDefaultLiteral<DateTimeLiteralToken>(context, reference, valueTypeDeclaration,
                    defaultValueExpression);
                break;

            default:
                throw new InvalidOperationException($"Unexpected type: {valueTypeDeclaration.Type}");
        }
    }

    private static void ValidateDefaultLiteral<T>(IValidationContext context, SourceReference reference,
        ValueTypeDeclaration valueTypeDeclaration,
        Expression defaultValueExpression)
        where T : ILiteralToken
    {
        if (defaultValueExpression is not LiteralExpression literalExpression)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{valueTypeDeclaration.Type}')");
            return;
        }

        if (literalExpression.Literal is not T)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{valueTypeDeclaration.Type}')");
        }
    }
}
