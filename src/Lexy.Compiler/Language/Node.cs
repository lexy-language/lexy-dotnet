using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public abstract class Node : INodeWithParent
{
    public SourceReference Reference { get; }

    public INode Parent { get; set; }

    protected Node(SourceReference reference)
    {
        Reference = Assert.NotNull(reference, nameof(reference));
    }

    public virtual void ValidateTree(IValidationContext context)
    {
        context.Visitor.Enter(this);

        ValidateChildren(context);
        Validate(context);

        context.Visitor.Leave(this);
    }

    public abstract IEnumerable<INode> GetChildren();

    void INodeWithParent.SetParent(INode node)
    {
        Parent = node;
    }

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
}
