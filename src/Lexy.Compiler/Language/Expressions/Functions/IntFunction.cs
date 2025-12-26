using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class IntFunction : SingleArgumentFunction
{
    public const string Name = "int";

    protected override string FunctionHelp => $"{Name} expects 1 argument (Value)";

    private IntFunction(Expression valueExpression, ExpressionSource source)
        : base(Name, valueExpression, source, PrimitiveType.Number, PrimitiveType.Number)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new IntFunction(expression, source);
    }
}