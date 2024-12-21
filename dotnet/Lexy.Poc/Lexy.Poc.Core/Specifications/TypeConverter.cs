using System;
using Lexy.Poc.Core.Compiler;

namespace Lexy.Poc.Core.Specifications
{
    internal static class TypeConverter
    {
        public static object Convert(ExecutionEnvironment environment, string value, string type)
        {
            if (environment.ContainsEnum(type))
            {
                var indexOfSeparator = value.IndexOf(".");
                var enumValue = value[(indexOfSeparator + 1)..];
                return Enum.Parse(environment.GetEnumType(type), enumValue);
            }

            return type switch
            {
                TypeNames.Number => decimal.Parse(value),
                TypeNames.DateTime => DateTime.Parse(value),
                TypeNames.Boolean => bool.Parse(value),
                TypeNames.String => value,
                _ => throw new InvalidOperationException($"Invalid type: {type}")
            };
        }
    }
}