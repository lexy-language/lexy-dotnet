using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioResults : ParsableNode
    {
        public IList<AssignmentDefinition> Assignments { get; } = new List<AssignmentDefinition>();

        public ScenarioResults(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return this;

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