using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.RunTime;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal class MinutesFunctionCall : EndStartDateFunctionCall<MinutesFunction>
{
    protected override string ClassName => nameof(BuiltInDateFunctions);
    protected override string MethodName => nameof(BuiltInDateFunctions.Minutes);
}