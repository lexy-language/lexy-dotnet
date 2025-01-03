using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class IntFunction : SingleArgumentFunction
{
    public const string Name = "INT";

    protected override string FunctionHelp => $"{Name} expects 1 argument (Value)";

    private IntFunction(Expression valueExpression, SourceReference reference)
        : base(valueExpression, reference, PrimitiveType.Number, PrimitiveType.Number)
    {
    }

    public static ExpressionFunction Create(SourceReference reference, Expression expression)
    {
        return new IntFunction(expression, reference);
    }
}