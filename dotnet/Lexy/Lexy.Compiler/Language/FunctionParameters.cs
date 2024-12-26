using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionParameters : ParsableNode
    {
        public IList<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        public FunctionParameters(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var variableDefinition = VariableDefinition.Parse(context);
            if (variableDefinition != null)
            {
                Variables.Add(variableDefinition);
            }
            return this;
        }

        public override IEnumerable<INode> GetChildren() => Variables;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}