using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.TypeSystem.Objects;

namespace Lexy.Compiler.Language.TypeSystem;

public class EnumType : ObjectType
{
    private readonly EnumDefinition enumDefinition;

    public EnumType(EnumDefinition enumDefinition) : base(enumDefinition.Name)
    {
        this.enumDefinition = enumDefinition;
    }

    public override bool IsAssignableFrom(Type type)
    {
        return Equals(type);
    }

    protected bool Equals(EnumType other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EnumType)obj);
    }

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return enumDefinition;
    }

    protected override IEnumerable<IObjectMember> CreateMembers()
    {
        return enumDefinition.Members.Select(member => new ObjectVariable(member.Name, this));
    }
}
