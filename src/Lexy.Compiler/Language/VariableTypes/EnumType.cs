using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public class EnumType : TypeWithMembers
{
    public string Type { get; }
    public EnumDefinition Enum { get; }

    public EnumType(string type, EnumDefinition enumDefinition)
    {
        Type = type;
        Enum = enumDefinition;
    }

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

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return Enum.Members.Any(member => member.Name == name) ? this : null;
    }

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return componentNodes.GetEnum(Type);
    }

    public static VariableType Generic()
    {
        return new EnumType("generic", new EnumDefinition("generic", new SourceReference(new SourceFile("generic"), 1, 1)));
    }
}