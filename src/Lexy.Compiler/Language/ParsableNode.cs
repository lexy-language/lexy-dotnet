using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public abstract class ParsableNode : Node, IParsableNode
{
    protected ParsableNode(NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
    }

    public abstract IParsableNode Parse(IParseLineContext context);
}
