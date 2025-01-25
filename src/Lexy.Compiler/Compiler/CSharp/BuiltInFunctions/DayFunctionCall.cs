using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class DayFunctionCall : SingleArgumentFunctionCall<DayFunction>
{
    protected override string ClassName => nameof(BuiltInDateFunctions);
    protected override string MethodName => nameof(BuiltInDateFunctions.Day);
}