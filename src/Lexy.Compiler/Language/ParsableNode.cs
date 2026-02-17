using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language;

public abstract class ParsableNode : Node, IParsableNode
{
    protected ParsableNode(NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
    }

    public abstract IParsableNode Parse(IParseLineContext context);
}
