using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class PowerFunction : FunctionCallExpression
{
    public const string Name = "power";

    private string FunctionHelp => $"'{Name} expects 2 arguments (Number, Power).";

    public Expression NumberExpression { get; }
    public Expression PowerExpression { get; }

    private PowerFunction(Expression numberExpression, Expression powerExpression, ExpressionSource source)
        : base(Name, source)
    {
        NumberExpression = numberExpression;
        PowerExpression = powerExpression;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return NumberExpression;
        yield return PowerExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        context
            .ValidateType(NumberExpression, 1, "Number", PrimitiveType.Number, Reference, FunctionHelp)
            .ValidateType(PowerExpression, 2, "Power", PrimitiveType.Number, Reference, FunctionHelp);
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return PrimitiveType.Number;
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression numberExpression,
        Expression powerExpression)
    {
        return new PowerFunction(numberExpression, powerExpression, source);
    }
}