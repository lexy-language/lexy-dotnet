using System;
using System.Linq;
using System.Text;

namespace Lexy.Compiler.Compiler.CSharp
{
    public static class ClassNames
    {
        public static string FunctionClassName(string functionName)
        {
            return Normalize(functionName, LexyCodeConstants.FunctionClassPrefix);
        }

        public static string CustomClassName(string complexTypeName)
        {
            return Normalize(complexTypeName, LexyCodeConstants.ComplexTypeClassPrefix);
        }

        public static string TableClassName(string tableTypeName)
        {
            return Normalize(tableTypeName, LexyCodeConstants.TableClassPrefix);
        }

        public static string EnumClassName(string enumName)
        {
            return Normalize(enumName, LexyCodeConstants.EnumClassPrefix);
        }

        private static string Normalize(string functionName, string functionClassPrefix)
        {
            var nameBuilder = new StringBuilder(functionClassPrefix);
            foreach (var @char in functionName.Where(ValidCharacter))
            {
                nameBuilder.Append(@char);
            }

            return nameBuilder.ToString();
        }

        private static bool ValidCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}