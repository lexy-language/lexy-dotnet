using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class AssignmentDefinitionState
{
    public Type Type { get; }

    public AssignmentDefinitionState(Type type)
    {
        Type = type;
    }
}

public class AssignmentDefinition : Node, IAssignmentDefinition
{
    private readonly Expression targetExpression;
    private readonly Expression variableExpression;

    public ConstantValue ConstantValue { get; }
    public IdentifierPath Variable { get; }

    public AssignmentDefinitionState State { get; private set; }

    public AssignmentDefinition(IdentifierPath variable, ConstantValue constantValue, Expression variableExpression,
        Expression targetExpression, NodeReference parentReference, SourceReference reference)
        : base(parentReference, reference)
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

        State = new AssignmentDefinitionState(context.VariableContext.GetType(Variable));

        if (expressionType != null && !expressionType.Equals(State.Type))
        {
            context.Logger.Fail(Reference,
                $"Variable '{Variable}' of type '{State}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        yield return this;
    }

    public override Symbol GetSymbol() => null;
}
