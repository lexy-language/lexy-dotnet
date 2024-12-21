using System;
using System.Collections.Generic;

namespace Lexy.Poc.Core
{
    public static class TypeNames
    {
        private static readonly IList<string> existing = new List<string>
        {
            Number,
            Boolean,
            DateTime,
            String
        };

        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string DateTime = "datetime";
        public const string String = "string";

        public static bool Contains(string parameterType)
        {
            return existing.Contains(parameterType);
        }

        public static Types ConvertToType(string value)
        {
            return value switch
            {
                Number => Types.Number,
                Boolean => Types.Boolean,
                DateTime => Types.DateTime,
                String => Types.String,
                _ => throw new InvalidOperationException("Invalid type: " + value)
            };
        }
    }
}