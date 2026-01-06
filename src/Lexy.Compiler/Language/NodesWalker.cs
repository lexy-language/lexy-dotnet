using System;
using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
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
}