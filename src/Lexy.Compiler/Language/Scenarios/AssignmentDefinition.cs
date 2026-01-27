using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.Scenarios;

public class AssignmentDefinition : Node, IAssignmentDefinition
{
    private readonly Expression targetExpression;
    private readonly Expression variableExpression;

    public ConstantValue ConstantValue { get; }
    public IdentifierPath Variable { get; }

    public Type Type { get; private set; }

    public AssignmentDefinition(IdentifierPath variable, ConstantValue constantValue, Expression variableExpression,
        Expression targetExpression, SourceReference reference)
        : base(reference)
    {
        Variable = variable;
        ConstantValue = constantValue;

        this.variableExpression = variableExpression;
        this.targetExpression = targetExpression;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return variableExpression;
        yield return targetExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        if (!context.VariableContext.Contains(Variable))
        {
            //logged by IdentifierExpressionValidation
            return;
        }

        var expressionType = targetExpression.DeriveType(context);

        Type = context.VariableContext.GetType(Variable);
        if (expressionType != null && !expressionType.Equals(Type))
        {
            context.Logger.Fail(Reference,
                $"Variable '{Variable}' of type '{Type}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        yield return this;
    }

    public override Symbol GetSymbol() => null;
}
