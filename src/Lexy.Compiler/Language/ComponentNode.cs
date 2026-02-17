namespace Lexy.Compiler.Language;

public abstract class ComponentNode : ParsableNode, IComponentNode
{
    public string Name { get; }

    protected ComponentNode(string name, NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
