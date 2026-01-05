using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Language.VariableTypes;
using Table = Lexy.Compiler.Language.Tables.Table;

namespace Lexy.Compiler.Language;

public class ComponentNodeList : IComponentNodeList
{
    private readonly IList<IComponentNode> values;

    public ComponentNodeList(params IComponentNode[] values)
    {
        this.values = values != null ? values.ToList() : new List<IComponentNode>();
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
        values.Add(componentNode);
    }

    internal bool ContainsEnum(string enumName)
    {
        return values
            .OfType<EnumDefinition>()
            .Any(definition => definition.Name.Value == enumName);
    }

    public IComponentNode GetNode(string name)
    {
        return values
            .FirstOrDefault(definition => definition.NodeName == name);
    }

    public bool Contains(string name)
    {
        return values
            .Any(definition => definition.NodeName == name);
    }

    public Function GetFunction(string name)
    {
        return values
            .OfType<Function>()
            .FirstOrDefault(function => function.Name.Value == name);
    }

    public Table GetTable(string name)
    {
        return values
            .OfType<Table>()
            .FirstOrDefault(function => function.Name.Value == name);
    }

    public TypeDefinition GetCustomType(string name)
    {
        return values
            .OfType<TypeDefinition>()
            .FirstOrDefault(function => function.Name.Value == name);
    }

    public IEnumerable<Scenario> GetScenarios()
    {
        return values.OfType<Scenario>();
    }

    public EnumDefinition GetEnum(string name)
    {
        return values
            .OfType<EnumDefinition>()
            .FirstOrDefault(enumDefinition => enumDefinition.Name.Value == name);
    }

    public void AddIfNew(IComponentNode node)
    {
        if (!values.Contains(node)) values.Add(node);
    }

    public INode First()
    {
        return values.FirstOrDefault();
    }

    public TypeWithMembers GetType(string name)
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