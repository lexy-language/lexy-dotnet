using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public abstract class Node : INode
{
    private readonly NodeReference parentReference;
    private SourceArea area;

    public SourceReference Reference { get; }

    public IReadonlySourceArea Area => area;

    public INode Parent => parentReference.Node;

    protected Node(NodeReference parentReference, SourceReference reference)
    {
        Reference = Assert.NotNull(reference, nameof(reference));
        this.parentReference = Assert.NotNull(parentReference, nameof(parentReference));
        area = new SourceArea(reference);
    }

    protected Node(INode parent, SourceReference reference) : this(new NodeReference(parent), reference)
    {
    }

    public virtual void ValidateTree(IValidationContext context)
    {
        context.Visitor.Enter(this);

        ValidateChildren(context);
        Validate(context);

        context.Visitor.Leave(this);
    }

    public abstract IEnumerable<INode> GetChildren();

    public abstract Symbol GetSymbol();

    public virtual SuggestionEdit[] GetSuggestions() => null;

    protected abstract void Validate(IValidationContext context);

    private void ValidateChildren(IValidationContext context)
    {
        var children = GetChildren();
        foreach (var child in children)
        {
            ValidateChild(context, child);
        }
    }

    protected virtual void ValidateChild(IValidationContext context, INode child)
    {
        child.ValidateTree(context);
    }

    public virtual void ExpandArea(Position position)
    {
        area.Expand(position);
        Parent?.ExpandArea(position);
    }

    protected bool Equals(Node other)
    {
        return Equals(Reference, other.Reference);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Node)obj);
    }

    public override int GetHashCode()
    {
        return (Reference != null ? Reference.GetHashCode() : 0);
    }
}
