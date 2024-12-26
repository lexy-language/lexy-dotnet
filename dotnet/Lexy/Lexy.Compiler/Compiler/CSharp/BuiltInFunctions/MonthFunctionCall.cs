using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class MonthFunctionCall : SingleArgumentFunctionCall
    {
        protected override string ClassName => nameof(BuiltInDateFunctions);
        protected override string MethodName => nameof(BuiltInDateFunctions.Month);

        public MonthFunctionCall(MonthFunction function) : base(function)
        {
        }
    }
}