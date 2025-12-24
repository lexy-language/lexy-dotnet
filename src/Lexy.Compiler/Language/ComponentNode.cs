using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public abstract class ComponentNode : ParsableNode, IComponentNode
{
    public abstract string NodeName { get; }

    protected ComponentNode(SourceReference reference) : base(reference)
    {
    }
}