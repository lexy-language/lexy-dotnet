using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public abstract class Node : INode
{
    protected Node(SourceReference reference)
    {
        Reference = reference ?? throw new ArgumentNullException(nameof(reference));
    }

    public SourceReference Reference { get; }

    public virtual void ValidateTree(IValidationContext context)
    {
        Validate(context);

        var children = GetChildren();
        foreach (var child in children)
        {
            ValidateNodeTree(context, child);
        }
    }

    public abstract IEnumerable<INode> GetChildren();

    protected virtual void ValidateNodeTree(IValidationContext context, INode child)
    {
        if (child == null) throw new InvalidOperationException($"({GetType().Name}) Child is null");
        child.ValidateTree(context);
    }

    protected abstract void Validate(IValidationContext context);
}