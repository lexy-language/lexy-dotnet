using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class TodayFunction : NoArgumentFunction
{
    public const string Name = "today";

    protected override VariableType ResultType => PrimitiveType.Date;

    private TodayFunction(ExpressionSource source)
        : base(Name, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source)
    {
        return new TodayFunction(source);
    }
}