using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioParameters : IToken
    {
        public IList<AssignmentDefinition> Assignments { get; } = new List<AssignmentDefinition>();

        public IToken Parse(Line line)
        {
            var assignment = AssignmentDefinition.Parse(line);
            Assignments.Add(assignment);
            return this;
        }
    }
}