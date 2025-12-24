using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.DependencyGraph;

public class Dependencies
{
    private readonly IComponentNodeList componentNodes;
    private readonly List<IComponentNode> circularReferences = new();
    private readonly Dictionary<string, DependencyNode> dependencyMap = new();
    private readonly Dictionary<string, IComponentNode> nodesMap = new();
    private readonly Dictionary<string, int> nodeOccurrences = new();
    private readonly Queue<IComponentNode> nodesToProcess;

    public IList<IComponentNode> SortedNodes { get; private set; }

    public IList<DependencyNode> DependencyNodes { get; } = new List<DependencyNode>();
    public bool HasCircularReferences => circularReferences.Count > 0;
    public IReadOnlyList<IComponentNode> CircularReferences => circularReferences;

    public Dependencies(IComponentNodeList componentNodes)
    {
        this.componentNodes = componentNodes ?? throw new ArgumentNullException(nameof(componentNodes));
        nodesToProcess = new Queue<IComponentNode>(this.componentNodes);
    }

    public void Build()
    {
        ProcessNodes();
        CheckCircularDependencies();
        SortedNodes = TopologicalSort();
    }

    public IEnumerable<IComponentNode> NodeAndDependencies(IComponentNode node)
    {
        return !dependencyMap.TryGetValue(node.NodeName, out var dependencyNode)
            ? new[] { node }
            : new[] { node }.Union(Flatten(dependencyNode.Dependencies));
    }

    private void ProcessNodes()
    {
        while (nodesToProcess.Count > 0)
        {
            var node = nodesToProcess.Dequeue();
            var dependencyNode = ProcessNode(node);
            DependencyNodes.Add(dependencyNode);
            dependencyMap.TryAdd(node.NodeName, dependencyNode);
            nodesMap.TryAdd(node.NodeName, node);
        }
    }

    private DependencyNode ProcessNode(IComponentNode componentNode)
    {
        IncreaseOccurrence(componentNode);
        return NewDependencyNode(componentNode);
    }

    private void IncreaseOccurrence(IComponentNode componentNode)
    {
        var key = componentNode.NodeName;
        if (nodeOccurrences.TryGetValue(key, out var existingOccurrences))
        {
            nodeOccurrences[key] = existingOccurrences + 1;
        }
        else
        {
            nodeOccurrences.Add(key, 1);
        }
    }

    private DependencyNode NewDependencyNode(IComponentNode componentNode)
    {
        var dependencies = GetDependencies(componentNode);
        return new DependencyNode(componentNode.NodeName, componentNode, dependencies);
    }

    private IReadOnlyList<string> GetDependencies(INode node)
    {
        var resultDependencies = new List<string>();
        NodesWalker.Walk(node, childNode => ProcessDependencies(childNode, resultDependencies));
        return resultDependencies;
    }

    private void ProcessDependencies(INode childNode, List<string> resultDependencies)
    {
        var nodeDependencies = (childNode as IHasNodeDependencies)?.GetDependencies(componentNodes);
        if (nodeDependencies == null) return;

        foreach (var dependency in nodeDependencies)
        {
            ValidateDependency(resultDependencies, dependency);
        }
    }

    private void ValidateDependency(List<string> resultDependencies, IComponentNode dependency)
    {
        if (resultDependencies.Contains(dependency.NodeName)) return;

        if (!nodesToProcess.Contains(dependency) && !nodesMap.ContainsKey(dependency.NodeName))
        {
            nodesToProcess.Enqueue(dependency);
        }

        IncreaseOccurrence(dependency);
        resultDependencies.Add(dependency.NodeName);
    }

    private void CheckCircularDependencies()
    {
        foreach (var node in DependencyNodes)
        {
            if (circularReferences.Contains(node.Node)) continue;
            if (IsCircular(node, node))
            {
                circularReferences.Add(node.Node);
            }
        }
    }

    private bool IsCircular(DependencyNode node, DependencyNode parent)
    {
        foreach (var dependencyNode in DependencyNodes)
        {
            if (dependencyNode == parent || !dependencyNode.HasDependency(parent)) continue;

            var dependency  = DependencyNodes.First(where => @where.Name == dependencyNode.Name);
            if (node.Name == dependency.Name) return true;
            if (IsCircular(node, dependency)) return true;
        }
        return false;
    }

    private IEnumerable<IComponentNode> Flatten(IReadOnlyList<string> dependencies)
    {
        var result = new List<IComponentNode>();
        Flatten(result, dependencies);
        return result;
    }

    private void Flatten(List<IComponentNode> result, IReadOnlyList<string> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            var dependencyNode = dependencyMap[dependency];
            if (result.Contains(dependencyNode.Node)) continue;
            result.Add(dependencyNode.Node);
            Flatten(result, dependencyNode.Dependencies);
        }
    }

    private IList<IComponentNode> TopologicalSort()
    {
        if (HasCircularReferences) return componentNodes.ToArray();

        var result = new List<IComponentNode>();
        var nodesWithoutDependants = nodeOccurrences
            .Where(pair => pair.Value == 1)
            .Select(pair => pair.Key);
        var processing = new Queue<string>(nodesWithoutDependants);

        while (processing.Count > 0) {
            var nodeName = processing.Dequeue();
            var node = nodesMap[nodeName];
            var dependencyNode = dependencyMap[nodeName];

            result.Insert(0, node);

            dependencyNode.Dependencies.ForEach(dependency =>
            {
                if (!nodeOccurrences.TryGetValue(dependency, out var occurrence) || occurrence < 1) return;

                var newOccurrence = occurrence - 1;
                nodeOccurrences[dependency] = newOccurrence;

                if (newOccurrence == 1) {
                    processing.Enqueue(dependency);
                }
            });
        }
        return result;
    }
}