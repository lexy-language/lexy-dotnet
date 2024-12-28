using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language
{
    public class FunctionResults : ParsableNode
    {
        public IList<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        public FunctionResults(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var variableDefinition = VariableDefinition.Parse(VariableSource.Results, context);
            if (variableDefinition == null) return this;

            if (variableDefinition.DefaultExpression != null)
            {
                context.Logger.Fail(Reference,
                    $"Result variable '{variableDefinition.Name}' should not have a default value.");
                return this;
            }

            Variables.Add(variableDefinition);

            return this;
        }

        public override IEnumerable<INode> GetChildren() => Variables;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}