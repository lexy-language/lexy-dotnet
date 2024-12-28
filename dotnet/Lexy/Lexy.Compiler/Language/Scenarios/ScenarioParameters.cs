using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language
{
    public class ScenarioParameters : ParsableNode
    {
        public IList<AssignmentDefinition> Assignments { get; } = new List<AssignmentDefinition>();

        public ScenarioParameters(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var assignment = AssignmentDefinition.Parse(context);
            if (assignment != null)
            {
                Assignments.Add(assignment);
            }
            return this;
        }

        public override IEnumerable<INode> GetChildren() => Assignments;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}