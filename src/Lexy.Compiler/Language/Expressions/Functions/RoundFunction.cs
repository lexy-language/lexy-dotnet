using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class RoundFunction : FunctionCallExpression
{
    public const string Name = "round";

    private string FunctionHelp => $"'{Name}' expects 2 arguments (Number, Digits).";

    public Expression NumberExpression { get; }
    public Expression DigitsExpression { get; }

    private RoundFunction(Expression numberExpression, Expression digitsExpression, ExpressionSource source)
        : base(Name, source)
    {
        NumberExpression = numberExpression;
        DigitsExpression = digitsExpression;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return NumberExpression;
        yield return DigitsExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        context
            .ValidateType(NumberExpression, 1, "Number", PrimitiveType.Number, Reference, FunctionHelp)
            .ValidateType(DigitsExpression, 2, "Digits", PrimitiveType.Number, Reference, FunctionHelp);
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return PrimitiveType.Number;
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression numberExpression,
        Expression powerExpression)
    {
        return new RoundFunction(numberExpression, powerExpression, source);
    }
}