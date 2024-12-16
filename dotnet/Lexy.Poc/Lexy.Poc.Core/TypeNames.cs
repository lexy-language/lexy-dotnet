using System.Collections.Generic;

namespace Lexy.Poc.Core
{
    internal static class TypeNames
    {
        private static readonly IList<string> existing = new List<string>
        {
            Int,
            Number,
            Boolean,
            DateTime
        };

        public const string Int = "int";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string DateTime = "datetime";

        public static bool Exists(string parameterType)
        {
            return existing.Contains(parameterType);
        }
    }
}