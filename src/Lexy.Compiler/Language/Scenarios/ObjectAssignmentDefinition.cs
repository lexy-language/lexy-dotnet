using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class ObjectAssignmentDefinition : ParsableNode, IAssignmentDefinition
{
    private readonly List<IAssignmentDefinition> assignments = new();

    public IdentifierPath Variable { get; }

    public IReadOnlyList<IAssignmentDefinition> Assignments => assignments;

    public ObjectAssignmentDefinition(IdentifierPath variable, NodeReference parentReference, SourceReference reference)
        : base(parentReference, reference)
    {
        Variable = variable;
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var assignment = AssignmentDefinitionParser.Parse(context, this, Variable);
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
        if (!context.VariableContext.Contains(Variable))
        {
            context.Logger.Fail(Reference, $"Variable '{Variable}' not found.");
        }

        var type = context.VariableContext.GetType(Variable);
        if (type is not DeclaredType && type is not GeneratedType)
        {
            context.Logger.Fail(Reference,
                $"Variable '{Variable}' without assignment should be an object type, but is '{type}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        return assignments.Flatten();
    }

    public override Symbol GetSymbol() => null;
}
