using System;
using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

internal static class NodesWalker
{
    public static void Walk(IEnumerable<INode> nodes, Action<INode> action)
    {
        Assert.NotNull(nodes, nameof(nodes));
        Assert.NotNull(action, nameof(action));

        foreach (var node in nodes)
        {
            Walk(node, action);
        }
    }

    public static void Walk(INode node, Action<INode> action)
    {
        Assert.NotNull(node, nameof(node));
        Assert.NotNull(action, nameof(action));

        action(node);

        var children = node.GetChildren();
        Walk(children, action);
    }
    
    
    public static void Walk(INode node, Action<INode, INode> action, INode parent)
    {
        Assert.NotNull(node, nameof(node));
        Assert.NotNull(action, nameof(action));

        action(node, parent);

        var children = node.GetChildren();
        Walk(children, action, node);
    }

    private static void Walk(IEnumerable<INode> children, Action<INode,INode> action, INode parent)
    {
        foreach (var node in children)
        {
            Walk(node, action, parent);
        }
    }
}
