using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions.Functions;

namespace Lexy.Compiler.DependencyGraph;

public static class DependencyGraphFactory
{
    public static Dependencies Create(RootNodeList rootNodes)
    {
        if (rootNodes == null) throw new ArgumentNullException(nameof(rootNodes));

        var dependencies = new Dependencies(rootNodes);
        dependencies.Build();
        return dependencies;
    }
}

public class Dependencies
{
    private readonly RootNodeList rootNodes;
    private readonly List<IRootNode> circularReferences = new List<IRootNode>();

    public IList<DependencyNode> Nodes { get; } = new List<DependencyNode>();
    public bool HasCircularReferences => circularReferences.Count > 0;
    public IReadOnlyList<IRootNode> CircularReferences => circularReferences;

    public Dependencies(RootNodeList rootNodes)
    {
        this.rootNodes = rootNodes ?? throw new ArgumentNullException(nameof(rootNodes));
    }

    public void Build()
    {
        ProcessNodes(rootNodes, null);
    }

    private void ProcessNodes(IEnumerable<IRootNode> nodes, DependencyNode parentNode)
    {
        foreach (var node in nodes)
        {
            Nodes.Add(ProcessNode(node, parentNode));
        }
    }

    private DependencyNode ProcessNode(INode node, DependencyNode parentNode)
    {
        var dependencyNode = NewDependencyNode(node, parentNode);
        var dependencies = GetDependencies(node, dependencyNode);
        foreach (var dependency in dependencies)
        {
            dependencyNode.AddDependency(dependency);
        }
        return dependencyNode;
    }

    private static DependencyNode NewDependencyNode(INode node, DependencyNode parentNode)
    {
        var name = node is IRootNode { NodeName: { } } rootNode ? rootNode.NodeName : node.GetType().Name;
        return new DependencyNode(name, node.GetType(), parentNode);
    }

    private IReadOnlyList<DependencyNode> GetDependencies(INode node, DependencyNode parentNode)
    {
        var resultDependencies = new List<DependencyNode>();
        NodesWalker.Walk(node, childNode => ProcessDependencies(parentNode, childNode, resultDependencies));
        return resultDependencies;
    }

    private void ProcessDependencies(DependencyNode parentNode, INode childNode, List<DependencyNode> resultDependencies)
    {
        var nodeDependencies = (childNode as IHasNodeDependencies)?.GetDependencies(rootNodes);
        if (nodeDependencies == null) return;

        foreach (var dependency in nodeDependencies)
        {
            ValidateDependency(parentNode, resultDependencies, dependency);
        }
    }

    private void ValidateDependency(DependencyNode parentNode, List<DependencyNode> resultDependencies, IRootNode dependency)
    {
        if (dependency == null) throw new InvalidOperationException("node.GetNodes() should never return null");

        if (parentNode != null && parentNode.ExistsInLineage(dependency.NodeName, dependency.GetType()))
        {
            circularReferences.Add(dependency);
        }
        else
        {
            if (DependencyExists(resultDependencies, dependency)) return;

            var dependencyNode = ProcessNode(dependency, parentNode);
            resultDependencies.Add(dependencyNode);
        }
    }

    private static bool DependencyExists(List<DependencyNode> resultDependencies, IRootNode dependency)
    {
        return resultDependencies.Any(any => any.Name == dependency.NodeName && any.Type == dependency.GetType());
    }
}

public class DependencyNode
{
    private readonly List<DependencyNode> dependencies = new();

    public string Name { get; }
    public Type Type { get; }
    public DependencyNode ParentNode { get; }

    public IReadOnlyList<DependencyNode> Dependencies => dependencies;

    public DependencyNode(string name, Type type, DependencyNode parentNode)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        ParentNode = parentNode;
    }

    public void AddDependency(DependencyNode dependency)
    {
        dependencies.Add(dependency);
    }

    protected bool Equals(DependencyNode other)
    {
        return Name == other.Name && Equals(Type, other.Type);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DependencyNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type);
    }

    public bool ExistsInLineage(string name, Type type)
    {
        if (Name == name && Type == type) return true;
        return ParentNode != null && ParentNode.ExistsInLineage(name, type);
    }
}
