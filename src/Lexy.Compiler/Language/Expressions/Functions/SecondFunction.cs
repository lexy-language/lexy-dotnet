using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class SecondFunction : SingleArgumentFunction
{
    public const string Name = "second";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private SecondFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new SecondFunction(expression, source);
    }
}