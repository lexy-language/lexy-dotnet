using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public sealed class CustomVariableDeclarationType : VariableDeclarationType, IHasNodeDependencies
{
    public string Type { get; }

    public CustomVariableDeclarationType(string type, SourceReference reference) : base(reference)
    {
        Type = type;
    }

    private bool Equals(CustomVariableDeclarationType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CustomVariableDeclarationType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var type = GetVariableType(componentNodes);
        return type switch
        {
            CustomType customType => new[] { customType.TypeDefinition },
            ComplexType complexType => new[] { complexType.Node },
            _ => Array.Empty<IComponentNode>()
        };
    }

    protected override VariableType ValidateVariableType(IValidationContext context)
    {
        var type = GetVariableType(context.ComponentNodes);
        if (type == null)
        {
            context.Logger.Fail(Reference, "Invalid type: '" + Type + "'");
        }
        return type;
    }

    private VariableType GetVariableType(IComponentNodeList componentNodes)
    {
        if (!Type.Contains('.'))
        {
            return componentNodes.GetType(Type);
        }

        var parts = Type.Split(".");
        if (parts.Length > 2)
        {
            return null;
        }

        var parent = componentNodes.GetType(parts[0]);
        if (parent == null)
        {
            return null;
        }

        return parent.MemberType(parts[1], componentNodes);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }
}