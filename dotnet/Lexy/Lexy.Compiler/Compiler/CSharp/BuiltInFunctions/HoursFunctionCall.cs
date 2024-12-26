using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class HoursFunctionCall : EndStartDateFunctionCall
    {
        protected override string ClassName => nameof(BuiltInDateFunctions);
        protected override string MethodName => nameof(BuiltInDateFunctions.Hours);

        public HoursFunctionCall(HoursFunction function) : base(function)
        {
        }
    }
}