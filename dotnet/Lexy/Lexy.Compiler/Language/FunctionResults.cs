using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionResults : ParsableNode
    {
        public IList<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        public FunctionResults(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var variableDefinition = VariableDefinition.Parse(context);
            if (variableDefinition == null) return this;

            if (variableDefinition.Default != null)
            {
                context.Logger.Fail(Reference,
                    $"Result variable '{variableDefinition.Name}' should not have a default value.");
                return this;
            }

            Variables.Add(variableDefinition);

            return this;
        }

        public VariableDeclarationType GetParameterType(string expectedName)
        {
            return Variables.FirstOrDefault(variable => variable.Name == expectedName)?.Type;
        }

        public override IEnumerable<INode> GetChildren() => Variables;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}