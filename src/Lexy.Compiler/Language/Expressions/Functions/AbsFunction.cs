using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class AbsFunction : SingleArgumentFunction
{
    public const string Name = "abs";

    protected override string FunctionHelp => $"{Name} expects 1 argument (Value)";

    private AbsFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Number, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new AbsFunction(expression, source);
    }
}