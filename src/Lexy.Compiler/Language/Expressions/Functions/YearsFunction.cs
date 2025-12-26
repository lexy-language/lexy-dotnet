namespace Lexy.Compiler.Language.Expressions.Functions;

public class YearsFunction : EndStartDateFunction
{
    public const string Name = "years";

    private YearsFunction(Expression endDateExpression, Expression startDateExpression, ExpressionSource source)
        : base(Name, endDateExpression, startDateExpression, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression endDateExpression,
        Expression startDateExpression)
    {
        return new YearsFunction(endDateExpression, startDateExpression, source);
    }
}