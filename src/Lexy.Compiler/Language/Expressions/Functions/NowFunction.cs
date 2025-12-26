using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class NowFunction : NoArgumentFunction
{
    public const string Name = "now";

    protected override VariableType ResultType => PrimitiveType.Date;

    private NowFunction(ExpressionSource source)
        : base(Name, source)
    {
    }

    public static FunctionCallExpression Create(ExpressionSource source)
    {
        return new NowFunction(source);
    }
}