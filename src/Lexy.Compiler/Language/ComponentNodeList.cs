using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Language;

public class ComponentNodeList : IComponentNodeList
{
    private readonly IList<IComponentNode> values;
    private readonly IDictionary<string, IComponentNode> index;

    public int Count => values.Count;

    public IComponentNode this[int index] => values[index];

    public ComponentNodeList(params IComponentNode[] values)
    {
        this.values = new List<IComponentNode>(values);
        index = values.ToDictionary(value => value.NodeName, value => value);
    }

    public IEnumerator<IComponentNode> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(IComponentNode componentNode)
    {
        Assert.NotNull(componentNode, nameof(componentNode));

        values.Add(componentNode);
        index.TryAdd(componentNode.NodeName, componentNode);
    }

    internal bool ContainsEnum(string enumName)
    {
        return index.TryGetValue(enumName, out var value) && value is EnumDefinition;
    }

    public IComponentNode GetNode(string name)
    {
        return index.TryGetValue(name, out var value) ? value : null;
    }

    public bool Contains(string name)
    {
        return index.TryGetValue(name, out var value);
    }

    public Function GetFunction(string name)
    {
        return index.TryGetValue(name, out var value) ? value as Function : null;
    }

    public Table GetTable(string name)
    {
        return index.TryGetValue(name, out var value) ? value as Table : null;
    }

    public TypeDefinition GetCustomType(string name)
    {
        return index.TryGetValue(name, out var value) ? value as TypeDefinition : null;
    }

    public IEnumerable<Scenario> GetScenarios()
    {
        return values.OfType<Scenario>();
    }

    public EnumDefinition GetEnum(string name)
    {
        return index.TryGetValue(name, out var value) ? value as EnumDefinition : null;
    }

    public void AddIfNew(IComponentNode node)
    {
        if (!values.Contains(node))
        {
            Add(node);
        }
    }

    public ObjectType GetType(string name)
    {
        var node = GetNode(name);
        return node switch
        {
            Table table => new TableType(name, table),
            Function function => new FunctionType(name, function),
            EnumDefinition enumDefinition => new EnumType(name, enumDefinition),
            TypeDefinition typeDefinition => new DeclaredType(name, typeDefinition),
            _ => null
        };
    }
}
