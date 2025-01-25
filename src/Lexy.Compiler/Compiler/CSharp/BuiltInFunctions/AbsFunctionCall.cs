using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class AbsFunctionCall : SingleArgumentFunctionCall<AbsFunction>
{
    protected override string ClassName => nameof(BuiltInNumberFunctions);
    protected override string MethodName => nameof(BuiltInNumberFunctions.Abs);
}