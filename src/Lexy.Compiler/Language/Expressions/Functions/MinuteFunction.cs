using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MinuteFunction : SingleArgumentFunction
{
    public const string Name = "minute";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private MinuteFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new MinuteFunction(expression, source);
    }
}