using System;
using System.Globalization;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

namespace Lexy.Compiler.Specifications;

internal static class TypeConverter
{
    public static object Convert(ICompilationResult compilationResult, object value, Type type)
    {
        Assert.NotNull(compilationResult, nameof(compilationResult));
        Assert.NotNull(value, nameof(value));
        Assert.NotNull(type, nameof(type));

        if (type is EnumType enumVariableType)
        {
            return ConvertEnum(compilationResult, value, enumVariableType);
        }

        if (type is ValueType primitiveVariableType)
        {
            return ConvertPrimitive(value, primitiveVariableType);
        }

        throw new InvalidOperationException($"Invalid type: '{type}'");
    }

    private static object ConvertPrimitive(object value, ValueType valueVariableType)
    {
        var valueAsString = value.ToString();
        return valueVariableType.Type switch
        {
            TypeNames.Number => value as decimal? ?? decimal.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Date => value as DateTime? ?? DateTime.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Boolean => value as bool? ?? bool.Parse(valueAsString),
            TypeNames.String => value,
            _ => throw new InvalidOperationException($"Invalid type: '{valueVariableType.Type}'")
        };
    }

    private static object ConvertEnum(ICompilationResult compilationResult, object value, EnumType enumVariableType)
    {
        var enumType = compilationResult.GetEnumType(enumVariableType.Name);
        if (enumType == null) throw new InvalidOperationException($"Unknown enum: {enumVariableType.Name}");

        var enumValueName = value.ToString();
        var indexOfSeparator = enumValueName.IndexOf(".", StringComparison.InvariantCulture);
        var enumValue = enumValueName[(indexOfSeparator + 1)..];
        return Enum.Parse(enumType, enumValue);
    }
}
