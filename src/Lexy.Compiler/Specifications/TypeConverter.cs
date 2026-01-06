using System;
using System.Globalization;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;

namespace Lexy.Compiler.Specifications;

internal static class TypeConverter
{
    public static object Convert(ICompilationResult compilationResult, object value, VariableType type)
    {
        Assert.NotNull(compilationResult, nameof(compilationResult));
        Assert.NotNull(value, nameof(value));
        Assert.NotNull(type, nameof(type));

        if (type is EnumType enumVariableType)
        {
            return ConvertEnum(compilationResult, value, enumVariableType);
        }

        if (type is PrimitiveType primitiveVariableType)
        {
            return ConvertPrimitive(value, primitiveVariableType);
        }

        throw new InvalidOperationException($"Invalid type: '{type}'");
    }

    private static object ConvertPrimitive(object value, PrimitiveType primitiveVariableType)
    {
        var valueAsString = value.ToString();
        return primitiveVariableType.Type switch
        {
            TypeNames.Number => value as decimal? ?? decimal.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Date => value as DateTime? ?? DateTime.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Boolean => value as bool? ?? bool.Parse(valueAsString),
            TypeNames.String => value,
            _ => throw new InvalidOperationException($"Invalid type: '{primitiveVariableType.Type}'")
        };
    }

    private static object ConvertEnum(ICompilationResult compilationResult, object value, EnumType enumVariableType)
    {
        var enumType = compilationResult.GetEnumType(enumVariableType.Type);
        if (enumType == null) throw new InvalidOperationException($"Unknown enum: {enumVariableType.Type}");

        var enumValueName = value.ToString();
        var indexOfSeparator = enumValueName.IndexOf(".", StringComparison.InvariantCulture);
        var enumValue = enumValueName[(indexOfSeparator + 1)..];
        return Enum.Parse(enumType, enumValue);
    }
}