namespace Lexy.Compiler.Language.Expressions.Functions;

public class SecondsFunction : EndStartDateFunction
{
    public const string Name = "seconds";

    private SecondsFunction(Expression endDateExpression, Expression startDateExpression, ExpressionSource source)
        : base(Name, endDateExpression, startDateExpression, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression endDateExpression,
        Expression startDateExpression)
    {
        return new SecondsFunction(endDateExpression, startDateExpression, source);
    }
}