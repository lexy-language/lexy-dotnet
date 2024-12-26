using Lexy.Poc.Core.Language.Expressions.Functions;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal class IntFunctionCall : SingleArgumentFunctionCall
    {
        protected override string ClassName => nameof(BuiltInNumberFunctions);
        protected override string MethodName => nameof(BuiltInNumberFunctions.Int);

        public IntFunctionCall(IntFunction function) : base(function)
        {
        }
    }
}