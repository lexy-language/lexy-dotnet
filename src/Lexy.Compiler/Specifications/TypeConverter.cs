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

        if (type is EnumType enumType)
        {
            return ConvertEnum(compilationResult, value, enumType);
        }

        if (type is ValueType valueType)
        {
            return ConvertValue(value, valueType);
        }

        throw new InvalidOperationException($"Invalid type: '{type}'");
    }

    private static object ConvertValue(object value, ValueType valueType)
    {
        var valueAsString = value.ToString();
        return valueType.Name switch
        {
            TypeNames.Number => value as decimal? ?? decimal.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Date => value as DateTime? ?? DateTime.Parse(valueAsString, CultureInfo.InvariantCulture),
            TypeNames.Boolean => value as bool? ?? bool.Parse(valueAsString),
            TypeNames.String => value,
            _ => throw new InvalidOperationException($"Invalid type: '{valueType.Name}'")
        };
    }

    private static object ConvertEnum(ICompilationResult compilationResult, object value, EnumType enumDefinitionType)
    {
        var enumType = compilationResult.GetEnumType(enumDefinitionType.Name);
        if (enumType == null) throw new InvalidOperationException($"Unknown enum: {enumType.Name}");

        var enumValueName = value.ToString();
        var indexOfSeparator = enumValueName.IndexOf(".", StringComparison.InvariantCulture);
        var enumValue = enumValueName[(indexOfSeparator + 1)..];
        return Enum.Parse(enumType, enumValue);
    }
}
