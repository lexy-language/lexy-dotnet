using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class MonthFunction : SingleArgumentFunction
{
    public const string Name = "MONTH";

    protected override string FunctionHelp => $"'{Name} expects 1 argument (Date)";

    private MonthFunction(Expression valueExpression, SourceReference reference)
        : base(valueExpression, reference, PrimitiveType.Date, PrimitiveType.Number)
    {
    }

    public static ExpressionFunction Create(SourceReference reference, Expression expression)
    {
        return new MonthFunction(expression, reference);
    }
}