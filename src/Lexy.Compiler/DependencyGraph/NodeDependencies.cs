using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public class NodeDependencies
{
    private int? occurrence;

    public IComponentNode Node { get; }

    public string Name => Node.Name;

    public readonly Dictionary<string, IComponentNode> Dependencies = new();
    public readonly Dictionary<string, IComponentNode> Dependants = new();

    internal NodeDependencies(IComponentNode node) => Node = Assert.NotNull(node, nameof(node));

    public void AddDependencies(IEnumerable<IComponentNode> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            Dependencies.TryAdd(dependency.Name, dependency);
        }
    }

    public void AddDependant(IComponentNode componentNode)
    {
        Dependants.TryAdd(componentNode.Name, componentNode);
    }

    public int DecreaseOccurrence()
    {
        if (occurrence == null)
        {
            occurrence = Dependants.Count;
        }
        else
        {
            occurrence -= 1;
        }

        return occurrence.Value;
    }

    public override string ToString() => $"{Node.Name} (dependencies: {Dependencies.Count} dependants: {Dependants.Count})";
}
