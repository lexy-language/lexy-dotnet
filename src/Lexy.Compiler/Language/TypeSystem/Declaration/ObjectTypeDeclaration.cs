using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Declaration;

//Syntax: "Function.Parameters variableName"
//Syntax: "Function.Row variableName"
public sealed class ObjectTypeDeclaration : TypeDeclaration, IHasNodeDependencies
{
    public string TypeName { get; }

    public ObjectTypeDeclaration(string typeName, SourceReference reference) : base(reference)
    {
        TypeName = typeName;
    }

    private bool Equals(ObjectTypeDeclaration other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ObjectTypeDeclaration)obj);
    }

    public override int GetHashCode()
    {
        return TypeName != null ? TypeName.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return TypeName;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var type = GetType(componentNodes);
        if (type is ObjectType objectType)
        {
            return objectType.GetDependencies(componentNodes);
        }
        return Array.Empty<IComponentNode>();
    }

    protected override Type ValidateType(IValidationContext context)
    {
        var type = GetType(context.ComponentNodes);
        if (type == null)
        {
            context.Logger.Fail(Reference, $"Invalid type: '{TypeName}'");
        }
        return type;
    }

    private Type GetType(IComponentNodeList componentNodes)
    {
        if (!TypeName.Contains('.'))
        {
            return componentNodes.GetType(TypeName);
        }

        var parts = TypeName.Split(".");
        if (parts.Length > 2) return null;

        var parent =  componentNodes.GetType(parts[0]);
        return parent?.MemberType(parts[1]);
    }

    public IComponentNode GetNode(IComponentNodeList componentNodes)
    {
        if (!TypeName.Contains('.'))
        {
            return componentNodes.GetNode(TypeName);
        }

        var parts = TypeName.Split(".");
        if (parts.Length > 2) return null;

        return componentNodes.GetNode(parts[0]);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }
}
