

using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class RootNode : ParsableNode, IRootNode
    {
        public abstract string NodeName { get; }

        protected RootNode(SourceReference reference) : base(reference)
        {
        }
    }
}