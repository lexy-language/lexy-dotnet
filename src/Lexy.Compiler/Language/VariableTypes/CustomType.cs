using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public class CustomType : TypeWithMembers
{
    public string Type { get; }
    public ITypeDefinition TypeDefinition { get; }

    public CustomType(string type, ITypeDefinition typeDefinition)
    {
        Type = type;
        TypeDefinition = typeDefinition;
    }

    protected bool Equals(CustomType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CustomType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        var definition = TypeDefinition.Variables.FirstOrDefault(variable => variable.Name == name);
        return definition?.Type.VariableType;
    }

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return TypeDefinition;
    }
}