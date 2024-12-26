using System;
using System.Collections.Generic;
using System.Linq;
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
}