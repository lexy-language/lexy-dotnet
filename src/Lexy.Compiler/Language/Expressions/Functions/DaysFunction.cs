using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class DaysFunction : EndStartDateFunction
{
    public const string Name = "days";

    private DaysFunction(Expression endDateExpression, Expression startDateExpression, ExpressionSource source)
        : base(Name, endDateExpression, startDateExpression, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression endDateExpression,
        Expression startDateExpression)
    {
        return new DaysFunction(endDateExpression, startDateExpression, source);
    }
}