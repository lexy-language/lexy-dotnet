using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public abstract class ComponentNode : ParsableNode, IComponentNode
{
    public abstract string Name { get; }

    protected ComponentNode(SourceReference reference) : base(reference)
    {
    }
}
