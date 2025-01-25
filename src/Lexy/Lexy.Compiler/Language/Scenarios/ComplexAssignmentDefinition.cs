using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ComplexAssignmentDefinition : ParsableNode, IAssignmentDefinition
{
    private readonly IList<IAssignmentDefinition> assignments = new List<IAssignmentDefinition>();

    public VariableReference Variable { get; }

    public ComplexAssignmentDefinition(VariableReference variable, SourceReference reference)
        : base(reference)
    {
        Variable = variable;
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var assignment = AssignmentDefinition.Parse(context, Variable);
        if (assignment == null) return this;

        assignments.Add(assignment);

        return assignment is IParsableNode parsableNode ? parsableNode : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return assignments;
    }

    protected override void Validate(IValidationContext context)
    {
        if (!context.VariableContext.Contains(Variable, context))
        {
            context.Logger.Fail(Reference, $"Variable '{Variable}' not found.");
        }

        var variableType = context.VariableContext.GetVariableType(Variable, context);
        if (variableType is not CustomType)
        {
            context.Logger.Fail(Reference, $"Variable '{Variable}' without assignment should be a complex type, but is '{variableType}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        return assignments.Flatten();
    }
}