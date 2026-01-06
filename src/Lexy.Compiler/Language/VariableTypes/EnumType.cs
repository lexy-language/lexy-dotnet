using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public class EnumType : ObjectType
{
    public string Type { get; }
    public EnumDefinition Enum { get; }

    public EnumType(string type, EnumDefinition enumDefinition)
    {
        Type = type;
        Enum = enumDefinition;
    }

    public override IObjectTypeVariable GetVariable(string name) => null;
    public override IObjectTypeFunction GetFunction(string name) => null;

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return Enum.Members.Any(member => member.Name == name) ? this : null;
    }

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return componentNodes.GetEnum(Type);
    }

    public override bool IsAssignableFrom(VariableType type) => Equals(type);

    protected bool Equals(EnumType other)
    {
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EnumType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }
}