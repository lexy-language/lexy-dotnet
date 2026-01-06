using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public class DependencyNode
{
    public string Name { get; }

    public IComponentNode Node { get; }

    public IReadOnlyList<string> Dependencies { get; }

    public DependencyNode(string name, IComponentNode node, IReadOnlyList<string> dependencies)
    {
        Name = Assert.NotNull(name, nameof(name));
        Node = Assert.NotNull(node, nameof(node));
        Dependencies = Assert.NotNull(dependencies, nameof(dependencies));
    }

    public bool HasDependency(DependencyNode parent)
    {
        return Dependencies.Any(where => where == parent.Name);
    }
}