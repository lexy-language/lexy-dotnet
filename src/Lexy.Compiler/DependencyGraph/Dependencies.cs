 using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.DependencyGraph;

public class Dependencies
{
    private readonly IComponentNodeList componentNodes;
    private readonly Dictionary<string, IComponentNode> circularReferences = new();
    private readonly Dictionary<string, NodeDependencies> nodeDependencies = new();

    private readonly Dictionary<string, IComponentNode> nodesToProcess = new();

    public bool HasCircularReferences => circularReferences.Count > 0;

    public IReadOnlyList<IComponentNode> SortedNodes { get; private set; }

    public Dictionary<string, NodeDependencies> Nodes => nodeDependencies;
    public Dictionary<string, IComponentNode> CircularReferences => circularReferences;

    public Dependencies(IComponentNodeList componentNodes)
    {
        this.componentNodes = Assert.NotNull(componentNodes, nameof(componentNodes));
    }

    public void Build()
    {
        ProcessNodes();
        CheckCircularDependencies();
        SortedNodes = SortNodesBeforeItsDependants();
    }

    public IEnumerable<IComponentNode> NodeAndDependencies(IComponentNode node)
    {
        var dependencies = GetOrCreateNodeDependencies(node);
        return dependencies == null
            ? new[] { node }
            : new[] { node }.Union(Flatten(dependencies.Dependencies.Values));
    }

    private void ProcessNodes()
    {
        foreach (var node in componentNodes)
        {
            ProcessNode(node);
        }

        while (nodesToProcess.Count > 0)
        {
            var firstKey = nodesToProcess.Keys.First();
            nodesToProcess.Remove(firstKey, out var node);
            ProcessNode(node);
        }
    }

    private void ProcessNode(IComponentNode componentNode)
    {
        var nodeDependencies = GetOrCreateNodeDependencies(componentNode);

        var nodeDependenciesNodes = GetDependencies(componentNode);
        foreach (var dependency in nodeDependenciesNodes.Values)
        {
            if (!nodesToProcess.ContainsKey(dependency.Name) && !this.nodeDependencies.ContainsKey(dependency.Name))
            {
                nodesToProcess.Add(dependency.Name, dependency);
            }

            var dependencyNodeDependencies = GetOrCreateNodeDependencies(dependency);
            dependencyNodeDependencies.AddDependant(componentNode);
        }

        nodeDependencies.AddDependencies(nodeDependenciesNodes.Values);
    }

    private NodeDependencies GetOrCreateNodeDependencies(IComponentNode node)
    {
        if (nodeDependencies.TryGetValue(node.Name, out var value)) return value;

        value = new NodeDependencies(node);
        nodeDependencies[node.Name] = value;
        return value;
    }

    private Dictionary<string, IComponentNode> GetDependencies(IComponentNode node)
    {
        var resultDependencies = new Dictionary<string, IComponentNode>();
        ProcessNodeDependencies(node, resultDependencies);
        return resultDependencies;
    }

    private void ProcessNodeDependencies(INode childNode, Dictionary<string, IComponentNode> resultDependencies)
    {
        GetNodeDependencies(childNode, resultDependencies);

        var children = childNode.GetChildren();
        foreach (var child in children)
        {
            ProcessNodeDependencies(child, resultDependencies);
        }
    }

    private void GetNodeDependencies(INode childNode, Dictionary<string, IComponentNode> resultDependencies)
    {
        if (childNode is not IHasNodeDependencies nodeWithDependencies) return;

        var nodeDependencies = nodeWithDependencies.GetDependencies(componentNodes);

        foreach (var dependency in nodeDependencies)
        {
            if (!resultDependencies.ContainsKey(dependency.Name))
            {
                resultDependencies.Add(dependency.Name, dependency);
            }
        }
    }

    private void CheckCircularDependencies()
    {
        foreach (var nodeDependency in nodeDependencies)
        {
            if (circularReferences.ContainsKey(nodeDependency.Key)) continue;
            if (IsCircular(nodeDependency.Value, nodeDependency.Value))
            {
                circularReferences.Add(nodeDependency.Key, nodeDependency.Value.Node);
            }
        }
    }

    private bool IsCircular(NodeDependencies node, NodeDependencies dependant)
    {
        foreach (var dependencyNode in dependant.Dependants)
        {
            if (node.Name == dependencyNode.Key) return true;

            var dependencyNodeDependencies = nodeDependencies[dependencyNode.Key];
            if (IsCircular(node, dependencyNodeDependencies))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerable<IComponentNode> Flatten(IEnumerable<IComponentNode> dependencies)
    {
        var result = new List<IComponentNode>();
        Flatten(result, dependencies);
        return result;
    }

    private void Flatten(List<IComponentNode> result, IEnumerable<IComponentNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (result.Contains(node)) continue;
            result.Add(node);

            var dependencies = GetOrCreateNodeDependencies(node);
            Flatten(result, dependencies.Dependencies.Values);
        }
    }

    private IReadOnlyList<IComponentNode> SortNodesBeforeItsDependants()
    {
        if (HasCircularReferences) return componentNodes.ToArray();

        var result = new List<IComponentNode>();
        var nodesWithoutDependants = NodesWithoutDependants();
        var processing = new Queue<string>(nodesWithoutDependants);

        while (processing.Count > 0)
        {
            var nodeName = processing.Dequeue();
            var dependencyNode = nodeDependencies[nodeName];

            result.Insert(0, dependencyNode.Node);

            dependencyNode.Dependencies.Values.ForEach(dependency =>
            {
                var dependant = GetOrCreateNodeDependencies(dependency);
                var occurrences = dependant.DecreaseOccurrence();

                if (occurrences == 1)
                {
                    processing.Enqueue(dependency.Name);
                }
            });
        }
        return result;
    }

    private IEnumerable<string> NodesWithoutDependants()
    {
        return nodeDependencies
            .Where(pair => pair.Value.Dependants.Count == 0)
            .Select(pair => pair.Key);
    }
}
