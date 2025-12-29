using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types;

public class TypeDefinition : ComponentNode, ITypeDefinition, IHasNodeDependencies
{
    private readonly List<VariableDefinition> variables = new();

    public TypeName Name { get; } = new();
    public override string NodeName => Name.Value;

    public IReadOnlyList<VariableDefinition> Variables => variables;

    private TypeDefinition(string name, SourceReference reference) : base(reference)
    {
        Name.ParseName(name);
    }

    internal static TypeDefinition Parse(NodeName name, SourceReference reference)
    {
        return new TypeDefinition(name.Name, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Parameters, context);
        if (variableDefinition != null) variables.Add(variableDefinition);
        return this;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var dependencies = Variables.SelectMany(variable =>
            variable.Type is IHasNodeDependencies hasDependencies
            ? hasDependencies.GetDependencies(componentNodes)
            : Array.Empty<IComponentNode>());
        return dependencies;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Variables;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override void ValidateTree(IValidationContext context)
    {
        using (context.CreateVariableScope())
        {
            base.ValidateTree(context);
        }
    }
}