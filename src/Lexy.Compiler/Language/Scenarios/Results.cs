using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class Results : ParsableNode
{
    private readonly IList<IAssignmentDefinition> assignments = new List<IAssignmentDefinition>();

    public Results(Scenario parent, SourceReference reference) : base(new NodeReference(parent), reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var assignment = AssignmentDefinitionParser.Parse(context, this);
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

    public IReadOnlyList<AssignmentDefinition> AllAssignments()
    {
        return assignments.Flatten().ToList();
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, "results", "Scenario results variables used to validate the function result.", SymbolKind.Keyword);
    }

    public override string ToString() => assignments.Count.ToString();
}
