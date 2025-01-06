using System;
using System.Globalization;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Specifications;

internal static class TypeConverter
{
    public static object Convert(CompilationResult compilationResult, object value, VariableType type)
    {
        if (compilationResult == null) throw new ArgumentNullException(nameof(compilationResult));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (type == null) throw new ArgumentNullException(nameof(type));

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
        return primitiveVariableType.Type switch
        {
            TypeNames.Number => value as decimal? ?? decimal.Parse(value.ToString(), CultureInfo.InvariantCulture),
            TypeNames.Date => value as DateTime? ?? DateTime.Parse(value.ToString(), CultureInfo.InvariantCulture),
            TypeNames.Boolean => value as bool? ?? bool.Parse(value.ToString()),
            TypeNames.String => value,
            _ => throw new InvalidOperationException($"Invalid type: '{primitiveVariableType.Type}'")
        };
    }

    private static object ConvertEnum(CompilationResult compilationResult, object value, EnumType enumVariableType)
    {
        var enumType = compilationResult.GetEnumType(enumVariableType.Type);
        if (enumType == null) throw new InvalidOperationException($"Unknown enum: {enumVariableType.Type}");

        var enumValueName = value.ToString();
        var indexOfSeparator = enumValueName.IndexOf(".", StringComparison.InvariantCulture);
        var enumValue = enumValueName[(indexOfSeparator + 1)..];
        return Enum.Parse(enumType, enumValue);
    }
}