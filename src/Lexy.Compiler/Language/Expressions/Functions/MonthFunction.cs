using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MonthFunction : SingleArgumentFunction
{
    public const string Name = "month";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private MonthFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new MonthFunction(expression, source);
    }
}