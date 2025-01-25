using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public abstract class SingleArgumentFunction : ExpressionFunction
{
    protected abstract string FunctionHelp { get; }

    protected VariableType ArgumentType { get; }
    protected VariableType ResultType { get; }

    public Expression ValueExpression { get; }

    protected SingleArgumentFunction(Expression valueExpression, SourceReference reference,
        VariableType argumentType, VariableType resultType)
        : base(reference)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
        ResultType = resultType ?? throw new ArgumentNullException(nameof(resultType));
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        context.ValidateType(ValueExpression, 1, "Value", ArgumentType, Reference, FunctionHelp);
    }

    public override VariableType DeriveReturnType(IValidationContext context)
    {
        return ResultType;
    }
}