using System;
using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public static class DependencyGraphFactory
{
    public static Dependencies Create(ComponentNodeList componentNodes)
    {
        Assert.NotNull(componentNodes, nameof(componentNodes));

        var dependencies = new Dependencies(componentNodes);
        dependencies.Build();
        return dependencies;
    }

    public static IEnumerable<IComponentNode> NodeAndDependencies(IComponentNodeList componentNodes, IComponentNode node)
    {
        Assert.NotNull(componentNodes, nameof(componentNodes));
        Assert.NotNull(node, nameof(node));

        var dependencies = new Dependencies(componentNodes);
        dependencies.Build();
        return dependencies.NodeAndDependencies(node);
    }
}