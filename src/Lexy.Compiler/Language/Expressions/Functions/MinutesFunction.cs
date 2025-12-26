using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MinutesFunction : EndStartDateFunction
{
    public const string Name = "minutes";

    private MinutesFunction(Expression endDateExpression, Expression startDateExpression, ExpressionSource source)
        : base(Name, endDateExpression, startDateExpression, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression endDateExpression,
        Expression startDateExpression)
    {
        return new MinutesFunction(endDateExpression, startDateExpression, source);
    }
}