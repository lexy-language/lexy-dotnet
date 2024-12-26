using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class Node : INode
    {
        public SourceReference Reference { get; }

        protected Node(SourceReference reference)
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        public virtual void ValidateTree(IValidationContext context)
        {
            Validate(context);

            var children = GetChildren();
            foreach (var child in children)
            {
                ValidateNodeTree(context, child);
            }
        }

        protected virtual void ValidateNodeTree(IValidationContext context, INode child)
        {
            if (child == null) throw new InvalidOperationException($"({GetType().Name}) Child is null");

            if (child is IRootNode node)
            {
                context.Logger.SetCurrentNode(node);
            }

            child.ValidateTree(context);

            if (this is IRootNode)
            {
                context.Logger.SetCurrentNode(this as IRootNode);
            }
        }

        public abstract IEnumerable<INode> GetChildren();

        protected abstract void Validate(IValidationContext context);
    }
}