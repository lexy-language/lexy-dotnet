using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class Results : ParsableNode
{
    private readonly IList<IAssignmentDefinition> assignments = new List<IAssignmentDefinition>();

    public Results(SourceReference reference) : base(reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var assignment = AssignmentDefinitionParser.Parse(context);
        if (assignment != null) assignments.Add(assignment);
        return assignment is IParsableNode parsableNode ? parsableNode : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return assignments;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public IList<AssignmentDefinition> AllAssignments()
    {
        return assignments.Flatten().ToList();
    }
}
