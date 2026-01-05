using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexy.Compiler.Language.VariableTypes;

public class GeneratedType : ComplexType
{
    public string Name { get; }
    public GeneratedTypeSource Source { get; }
    public IEnumerable<ComplexTypeVariable> Members { get; }
    public IComponentNode Node { get;}

    public GeneratedType(string name, IComponentNode node, GeneratedTypeSource source, IEnumerable<ComplexTypeVariable> members)
    {
        Name = name;
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Source = source;
        Members = members ?? throw new ArgumentNullException(nameof(members));
    }

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return Members.FirstOrDefault(member => member.Name == name)?.Type;
    }

    public override IComplexTypeVariable GetVariable(string name)
    {
        return Members.FirstOrDefault(variable => variable.Name == name);
    }

    public override IComplexTypeFunction GetFunction(string name)
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