using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.DependencyGraph;

public class DependencyNode
{
    public string Name { get; }

    public IComponentNode Node { get; }

    public IReadOnlyList<string> Dependencies { get; }

    public DependencyNode(string name, IComponentNode node, IReadOnlyList<string> dependencies)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }

    public bool HasDependency(DependencyNode parent)
    {
        return Dependencies.Any(where => where == parent.Name);
    }
}