using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class YearFunction : SingleArgumentFunction
{
    public const string Name = "year";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private YearFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new YearFunction(expression, source);
    }
}