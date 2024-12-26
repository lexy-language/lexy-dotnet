using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionIncludes : ParsableNode
    {
        public IList<FunctionInclude> Definitions { get; } = new List<FunctionInclude>();

        public FunctionIncludes(SourceReference reference) : base(reference)
        {
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return this;

            var definition = FunctionInclude.Parse(context);
            if (definition != null)
            {
                Definitions.Add(definition);
            }
            return this;
        }

        public override IEnumerable<INode> GetChildren() => Definitions;

        protected override void Validate(IValidationContext context)
        {
        }
    }
}