using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class AbsFunction : SingleArgumentFunction
{
    public const string Name = "ABS";

    protected override string FunctionHelp => $"{Name} expects 1 argument (Value)";

    private AbsFunction(Expression valueExpression, SourceReference reference)
        : base(valueExpression, reference, PrimitiveType.Number, PrimitiveType.Number)
    {
    }

    public static ExpressionFunction Create(SourceReference reference, Expression expression)
    {
        return new AbsFunction(expression, reference);
    }
}