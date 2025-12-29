using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class AssignmentDefinition : Node, IAssignmentDefinition
{
    private readonly Expression targetExpression;
    private readonly Expression variableExpression;

    public ConstantValue ConstantValue { get; }
    public IdentifierPath Variable { get; }

    public VariableType VariableType { get; private set; }

    public AssignmentDefinition(Language.IdentifierPath variable, ConstantValue constantValue, Expression variableExpression,
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
        if (!context.VariableContext.Contains(Variable, context))
        {
            //logged by IdentifierExpressionValidation
            return;
        }

        var expressionType = targetExpression.DeriveType(context);

        VariableType = context.VariableContext.GetVariableType(Variable, context);
        if (expressionType != null && !expressionType.Equals(VariableType))
        {
            context.Logger.Fail(Reference,
                $"Variable '{Variable}' of type '{VariableType}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        yield return this;
    }
}