using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.VariableTypes;

public class GeneratedType : ObjectType
{
    public string Name { get; }
    public GeneratedTypeSource Source { get; }
    public IEnumerable<ObjectTypeVariable> Members { get; }
    public IComponentNode Node { get;}

    public GeneratedType(string name, IComponentNode node, GeneratedTypeSource source, IEnumerable<ObjectTypeVariable> members)
    {
        Name = name;
        Node = Assert.NotNull(node, nameof(node));
        Source = source;
        Members = Assert.NotNull(members, nameof(members));
    }

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return Members.FirstOrDefault(member => member.Name == name)?.Type;
    }

    public override IObjectTypeVariable GetVariable(string name)
    {
        return Members.FirstOrDefault(variable => variable.Name == name);
    }

    public override IObjectTypeFunction GetFunction(string name)
    {
        return null;
    }

    protected bool Equals(GeneratedType other)
    {
        return Name == other.Name && Source == other.Source;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GeneratedType)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Source);
    }

    public override string ToString()
    {
        return Name;
    }
}