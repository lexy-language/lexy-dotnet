namespace Lexy.Compiler.Language.Expressions.Functions;

public class HoursFunction : EndStartDateFunction
{
    public const string Name = "hours";

    private HoursFunction(Expression endDateExpression, Expression startDateExpression, ExpressionSource source)
        : base(Name, endDateExpression, startDateExpression, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression endDateExpression,
        Expression startDateExpression)
    {
        return new HoursFunction(endDateExpression, startDateExpression, source);
    }
}