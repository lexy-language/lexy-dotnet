using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions.Functions;

namespace Lexy.Compiler.Compiler.CSharp
{
    internal class CompileFunctionContext : ICompileFunctionContext
    {
        public Function Function { get; }
        public IEnumerable<FunctionCall> BuiltInFunctionCalls { get; }

        public CompileFunctionContext(Function function, IEnumerable<FunctionCall> builtInFunctionCalls)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            BuiltInFunctionCalls = builtInFunctionCalls ?? throw new ArgumentNullException(nameof(builtInFunctionCalls));
        }

        public FunctionCall Get(ExpressionFunction expressionFunction)
        {
            return BuiltInFunctionCalls.FirstOrDefault(call => call.ExpressionFunction == expressionFunction);
        }

        public string FunctionClassName() => ClassNames.FunctionClassName(Function.Name.Value);
    }

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

        private static string Normalize(string functionName, string functionClassPrefix)
        {
            var nameBuilder = new StringBuilder(functionClassPrefix);
            foreach (var @char in functionName.Where(char.IsLetter))
            {
                nameBuilder.Append(@char);
            }

            return nameBuilder.ToString();
        }
    }

}