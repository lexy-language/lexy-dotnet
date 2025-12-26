using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class DayFunction : SingleArgumentFunction
{
    public const string Name = "day";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private DayFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new DayFunction(expression, source);
    }
}