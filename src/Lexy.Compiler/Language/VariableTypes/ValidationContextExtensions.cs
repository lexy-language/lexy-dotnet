using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.VariableTypes;

public static class ValidationContextExtensions
{
    public static void ValidateTypeAndDefault(this IValidationContext context, SourceReference reference,
        VariableTypeDeclaration type, Expression defaultValueExpression)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (type == null) throw new ArgumentNullException(nameof(type));

        switch (type)
        {
            case ComplexVariableTypeDeclaration customVariableType:
                ValidateCustomVariableType(context, reference, customVariableType, defaultValueExpression);
                break;

            case PrimitiveVariableTypeDeclaration primitiveVariableType:
                ValidatePrimitiveVariableType(context, reference, primitiveVariableType, defaultValueExpression);
                break;

            default:
                throw new InvalidOperationException($"Invalid Type: {type.GetType()}");
        }
    }

    private static void ValidateCustomVariableType(IValidationContext context, SourceReference reference,
        ComplexVariableTypeDeclaration complexVariableTypeDeclaration, Expression defaultValueExpression)
    {
        var variablePathComplex = IdentifierPath.Parse(complexVariableTypeDeclaration.Type);
        var variable = context.VariableContext.CreateVariableReference(reference, variablePathComplex, context);
        var type = variable?.VariableType;
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
                $"Invalid default value '{defaultValueExpression}'. (type: '{complexVariableTypeDeclaration.Type}') does not support a default value.");
            return;
        }

        if (defaultValueExpression is not MemberAccessExpression memberAccessExpression
         || memberAccessExpression.VariablePath == null)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{complexVariableTypeDeclaration.Type}')");
            return;
        }

        var variablePath = memberAccessExpression.VariablePath;
        if (variablePath.Parts != 2)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{complexVariableTypeDeclaration.Type}')");
        }
        if (variablePath.RootIdentifier != complexVariableTypeDeclaration.Type)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. Invalid enum type. (type: '{complexVariableTypeDeclaration.Type}')");
        }

        var enumDeclaration = context.ComponentNodes.GetEnum(variablePath.RootIdentifier);
        if (enumDeclaration == null || !enumDeclaration.ContainsMember(variablePath.Path[1]))
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. Invalid member. (type: '{complexVariableTypeDeclaration.Type}')");
        }
    }

    private static void ValidatePrimitiveVariableType(IValidationContext context, SourceReference reference,
        PrimitiveVariableTypeDeclaration primitiveVariableTypeDeclaration, Expression defaultValueExpression)
    {
        if (defaultValueExpression == null) return;

        switch (primitiveVariableTypeDeclaration.Type)
        {
            case TypeNames.Number:
                ValidateDefaultLiteral<NumberLiteralToken>(context, reference, primitiveVariableTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.String:
                ValidateDefaultLiteral<QuotedLiteralToken>(context, reference, primitiveVariableTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.Boolean:
                ValidateDefaultLiteral<BooleanLiteralToken>(context, reference, primitiveVariableTypeDeclaration,
                    defaultValueExpression);
                break;

            case TypeNames.Date:
                ValidateDefaultLiteral<DateTimeLiteralToken>(context, reference, primitiveVariableTypeDeclaration,
                    defaultValueExpression);
                break;

            default:
                throw new InvalidOperationException($"Unexpected type: {primitiveVariableTypeDeclaration.Type}");
        }
    }

    private static void ValidateDefaultLiteral<T>(IValidationContext context, SourceReference reference,
        PrimitiveVariableTypeDeclaration primitiveVariableTypeDeclaration,
        Expression defaultValueExpression)
        where T : ILiteralToken
    {
        if (defaultValueExpression is not LiteralExpression literalExpression)
        {
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{primitiveVariableTypeDeclaration.Type}')");
            return;
        }

        if (literalExpression.Literal is not T)
            context.Logger.Fail(reference,
                $"Invalid default value '{defaultValueExpression}'. (type: '{primitiveVariableTypeDeclaration.Type}')");
    }
}