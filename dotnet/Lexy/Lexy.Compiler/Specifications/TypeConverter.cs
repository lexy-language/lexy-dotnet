using System;
using System.Globalization;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;
using Lexy.RunTime;

namespace Lexy.Compiler.Specifications
{
    internal static class TypeConverter
    {
        public static object Convert(CompilerResult compilerResult, string value, VariableDeclarationType type)
        {
            if (compilerResult == null) throw new ArgumentNullException(nameof(compilerResult));
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type is CustomVariableDeclarationType enumVariableType)
            {
                if (!compilerResult.ContainsEnum(enumVariableType.Type)) throw new InvalidOperationException("Known enum: " + enumVariableType.Type);

                var indexOfSeparator = value.IndexOf(".");
                var enumValue = value[(indexOfSeparator + 1)..];

                var enumType = compilerResult.GetEnumType(enumVariableType.Type);

                return EnumerationParser.Parse(enumType, enumValue);
            }

            if (type is PrimitiveVariableDeclarationType primitiveVariableType)
            {
                return primitiveVariableType.Type switch
                {
                    TypeNames.Number => decimal.Parse(value, CultureInfo.InvariantCulture),
                    TypeNames.Date => DateTime.Parse(value, CultureInfo.InvariantCulture),
                    TypeNames.Boolean => bool.Parse(value),
                    TypeNames.String => value,
                    _ => throw new InvalidOperationException($"Invalid type: '{primitiveVariableType.Type}'")
                };
            }

            throw new InvalidOperationException($"Invalid type: '{type}'");
        }
    }
}