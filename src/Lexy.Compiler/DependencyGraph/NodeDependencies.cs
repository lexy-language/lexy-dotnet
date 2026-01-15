using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public class NodeDependencies
{
    private int? occurence;

    public IComponentNode Node { get; }

    public string Name => Node.NodeName;

    public readonly Dictionary<string, IComponentNode> Dependencies = new();
    public readonly Dictionary<string, IComponentNode> Dependants = new();

    internal NodeDependencies(IComponentNode node) => Node = Assert.NotNull(node, nameof(node));

    public void AddDependencies(IEnumerable<IComponentNode> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            Dependencies.TryAdd(dependency.NodeName, dependency);
        }
    }

    public void AddDependant(IComponentNode componentNode)
    {
        Dependants.TryAdd(componentNode.NodeName, componentNode);
    }

    public int DecreaseOccurrence()
    {
        if (occurence == null)
        {
            occurence = Dependants.Count;
        }
        else
        {
            occurence -= 1;
        }

        return occurence.Value;
    }

    public override string ToString() => $"{Node.NodeName} (dependencies: {Dependencies.Count} dependants: {Dependants.Count})";
}
