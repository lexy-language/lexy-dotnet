using System;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language
{
    public static class ValidationContextExtensions
    {
        public static void ValidateTypeAndDefault(this IValidationContext context, SourceReference reference, VariableDeclarationType type, ILiteralToken defaultValue)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (type == null) throw new ArgumentNullException(nameof(type));

            switch (type)
            {
                case CustomVariableDeclarationType customVariableType:
                    ValidateEnumVariableType(context, reference, customVariableType, defaultValue);
                    break;

                case PrimitiveVariableDeclarationType primitiveVariableType:
                    ValidatePrimitiveVariableType(context, reference, primitiveVariableType,defaultValue);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid Type: {type.GetType()}");
            }
        }

        private static void ValidateEnumVariableType(IValidationContext context, SourceReference reference, CustomVariableDeclarationType customVariableDeclarationType, ILiteralToken defaultValue)
        {
            if (!context.Nodes.ContainsEnum(customVariableDeclarationType.Type))
            {
                context.Logger.Fail(reference, $"Unknown enum type: '{customVariableDeclarationType.Type}'");
                return;
            }

            if (defaultValue == null) return;

            if (!(defaultValue is MemberAccessLiteral memberAccessLiteral))
            {
                context.Logger.Fail(reference, $"Invalid default value '{defaultValue}'. (type: '{customVariableDeclarationType.Type}')");
                return;
            }

            var parts = memberAccessLiteral.GetParts();
            var enumDeclaration = context.Nodes.GetEnum(customVariableDeclarationType.Type);
            if (parts.Length != 2)
            {
                context.Logger.Fail(reference, $"Invalid default value '{defaultValue}'. (type: '{customVariableDeclarationType.Type}')");
            }
            if (parts[0] != customVariableDeclarationType.Type)
            {
                context.Logger.Fail(reference, $"Invalid default value '{defaultValue}'. Invalid enum type. (type: '{customVariableDeclarationType.Type}')");
            }
            if (!enumDeclaration.ContainsMember(parts[1]))
            {
                context.Logger.Fail(reference, $"Invalid default value '{defaultValue}'. Invalid member. (type: '{customVariableDeclarationType.Type}')");
            }
        }

        private static void ValidatePrimitiveVariableType(IValidationContext context, SourceReference reference, PrimitiveVariableDeclarationType primitiveVariableDeclarationType, ILiteralToken defaultValue)
        {
            if (defaultValue == null) return;

            switch (primitiveVariableDeclarationType.Type)
            {
                case TypeNames.Number:
                    ValidateDefaultLiteral<NumberLiteralToken>(context, reference, primitiveVariableDeclarationType, defaultValue);
                    break;

                case TypeNames.String:
                    ValidateDefaultLiteral<QuotedLiteralToken>(context, reference,primitiveVariableDeclarationType, defaultValue);
                    break;

                case TypeNames.Boolean:
                    ValidateDefaultLiteral<BooleanLiteral>(context, reference,primitiveVariableDeclarationType, defaultValue);
                    break;

                case TypeNames.Date:
                    ValidateDefaultLiteral<DateTimeLiteral>(context, reference, primitiveVariableDeclarationType, defaultValue);
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected type: {primitiveVariableDeclarationType.Type}");
            }
        }

        private static void ValidateDefaultLiteral<T>(IValidationContext context, SourceReference reference, PrimitiveVariableDeclarationType primitiveVariableDeclarationType, ILiteralToken defaultValue)
            where T : ILiteralToken
        {
            if (!(defaultValue is T))
            {
                context.Logger.Fail(reference, $"Invalid default value '{defaultValue}'. (type: '{primitiveVariableDeclarationType.Type}')");
            }
        }
    }
}