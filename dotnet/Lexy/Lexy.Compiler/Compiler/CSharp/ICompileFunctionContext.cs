using System.Collections.Generic;
using Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Language.Expressions.Functions;

namespace Lexy.Poc.Core.Compiler.CSharp
{
    internal interface ICompileFunctionContext
    {
        IEnumerable<BuiltInFunctionCall> BuiltInFunctionCalls { get; }

        BuiltInFunctionCall Get(BuiltInFunction expressionBuiltInFunction);
    }
}