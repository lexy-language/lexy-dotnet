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

    public IEnumerable<IRootNode> GetDependencies(RootNodeList rootNodeList)
    {
        return VariableType switch
        {
            CustomType customType => new[] { customType.TypeDefinition },
            ComplexType complexType => new[] { complexType.Node },
            _ => Array.Empty<IRootNode>()
        };
    }

    public override VariableType CreateVariableType(IValidationContext context)
    {
        if (!Type.Contains('.'))
        {
            return context.RootNodes.GetType(Type);
        }

        var parts = Type.Split(".");
        if (parts.Length > 2)
        {
            context.Logger.Fail(Reference, "Invalid type: '" + Type + "'");
            return null;
        }

        var parent = context.RootNodes.GetType(parts[0]);
        if (parent == null)
        {
            context.Logger.Fail(Reference, "Invalid type: '" + Type + "'");
            return null;
        }

        return parent.MemberType(parts[1], context);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        VariableType = CreateVariableType(context);
    }
}