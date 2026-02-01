using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Types;

public class TypeDefinition : ComponentNode, ITypeDefinition, IHasNodeDependencies, INodeWithType
{
    private readonly List<VariableDefinition> variables = new();

    public IReadOnlyList<VariableDefinition> Variables => variables;

    private TypeDefinition(string name, LexyScriptNode parentReference, SourceReference reference) :
        base(name, new NodeReference(parentReference), reference)
    {
    }

    public Type CreateType()
    {
        return new DeclaredType(this);
    }

    internal static TypeDefinition Parse(NodeName name, LexyScriptNode parent, SourceReference reference)
    {
        return new TypeDefinition(name.Name, parent, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var variableDefinition = VariableDefinition.Parse(VariableSource.Parameters, context, new NodeReference(this));
        if (variableDefinition != null) variables.Add(variableDefinition);
        return this;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var dependencies = Variables.SelectMany(variable =>
            variable.TypeDeclaration is IHasNodeDependencies hasDependencies
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
        context.InNodeVariableScope(this, base.ValidateTree);
    }

    public override Symbol GetSymbol()
    {
        var builder = new StringBuilder();
        foreach (var variable in Variables)
        {
            builder.AppendLine($"- {variable.TypeDeclaration} {variable.Name}");
        }
        var variablesString = builder.ToString();
        return new Symbol(Reference, "type: " + Name, variablesString, SymbolKind.Type);
    }
}
