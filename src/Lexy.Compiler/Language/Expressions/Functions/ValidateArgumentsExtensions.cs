using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Expressions.Functions;

public static class ValidateArgumentsExtensions
{
    public static IValidationContext ValidateType(this IValidationContext context, Expression expression,
        int argumentIndex, string name, Type type, SourceReference reference, string functionHelp)
    {
        var valueTypeEnd = expression.DeriveType(context);
        if (valueTypeEnd == null || !valueTypeEnd.Equals(type))
        {
            context.Logger.Fail(reference,
                $"Invalid argument {argumentIndex}. '{name}' should be of type '{type}' but is '{valueTypeEnd}'. {functionHelp}");
        }

        return context;
    }
}