using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class AbsFunctionCall : SingleArgumentFunctionCall
    {
        protected override string ClassName => nameof(BuiltInNumberFunctions);
        protected override string MethodName => nameof(BuiltInNumberFunctions.Abs);

        public AbsFunctionCall(AbsFunction function) : base(function)
        {
        }
    }
}