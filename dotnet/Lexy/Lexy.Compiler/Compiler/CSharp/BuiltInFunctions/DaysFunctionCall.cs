using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class DaysFunctionCall : EndStartDateFunctionCall
    {
        protected override string ClassName => nameof(BuiltInDateFunctions);
        protected override string MethodName => nameof(BuiltInDateFunctions.Days);

        public DaysFunctionCall(DaysFunction function) : base(function)
        {
        }
    }
}