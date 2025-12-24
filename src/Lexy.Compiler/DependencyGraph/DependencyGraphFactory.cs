using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.DependencyGraph;

public static class DependencyGraphFactory
{
    public static Dependencies Create(ComponentNodeList componentNodes)
    {
        if (componentNodes == null) throw new ArgumentNullException(nameof(componentNodes));

        var dependencies = new Dependencies(componentNodes);
        dependencies.Build();
        return dependencies;
    }

    public static IEnumerable<IComponentNode> NodeAndDependencies(IComponentNodeList componentNodes, IComponentNode node)
    {
        if (componentNodes == null) throw new ArgumentNullException(nameof(componentNodes));
        if (node == null) throw new ArgumentNullException(nameof(node));

        var dependencies = new Dependencies(componentNodes);
        dependencies.Build();
        return dependencies.NodeAndDependencies(node);
    }
}