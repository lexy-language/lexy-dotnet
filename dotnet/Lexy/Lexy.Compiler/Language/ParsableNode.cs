using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class ParsableNode : Node, IParsableNode
    {
        public abstract IParsableNode Parse(IParserContext context);

        protected ParsableNode(SourceReference reference) : base(reference)
        {
        }
    }
}