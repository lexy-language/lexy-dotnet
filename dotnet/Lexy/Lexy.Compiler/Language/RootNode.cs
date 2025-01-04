using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language;

public abstract class RootNode : ParsableNode, IRootNode
{
    public abstract string NodeName { get; }

    protected RootNode(SourceReference reference) : base(reference)
    {
    }

    protected override void ValidateNodeTree(IValidationContext context, INode child)
    {
        if (child == null) throw new InvalidOperationException($"({GetType().Name}) Child is null");

        if (child is IRootNode node)
        {
            context.Logger.SetCurrentNode(node);
        }

        child.ValidateTree(context);

        context.Logger.SetCurrentNode(this);
    }
}