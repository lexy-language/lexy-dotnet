using System;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Specifications
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

                return Enum.Parse(compilerResult.GetEnumType(enumVariableType.Type), enumValue);
            }

            if (type is PrimitiveVariableDeclarationType primitiveVariableType)
            {
                return primitiveVariableType.Type switch
                {
                    TypeNames.Number => decimal.Parse(value),
                    TypeNames.Date => DateTime.Parse(value),
                    TypeNames.Boolean => bool.Parse(value),
                    TypeNames.String => value,
                    _ => throw new InvalidOperationException($"Invalid type: '{primitiveVariableType.Type}'")
                };
            }

            throw new InvalidOperationException($"Invalid type: '{type}'");
        }
    }
}