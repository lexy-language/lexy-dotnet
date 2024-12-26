using System.Collections.Generic;

namespace Lexy.Poc.Core.Language
{
    public static class TypeNames
    {
        private static readonly IList<string> existing = new List<string>
        {
            Number,
            Boolean,
            Date,
            String
        };

        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string Date = "date";
        public const string String = "string";

        public static bool Contains(string parameterType)
        {
            return existing.Contains(parameterType);
        }
    }
}