using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Language.VariableTypes;

public class GeneratedType : VariableType, ITypeWithMembers
{
    public string Name { get; }
    public GeneratedTypeSource Source { get; }
    public IEnumerable<GeneratedTypeMember> Members { get; }
    public IComponentNode Node { get;}

    public GeneratedType(string name, IComponentNode node, GeneratedTypeSource source, IEnumerable<GeneratedTypeMember> members)
    {
        Name = name;
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Source = source;
        Members = members ?? throw new ArgumentNullException(nameof(members));
    }

    public VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return Members.FirstOrDefault(member => member.Name == name)?.Type;
    }

    public IInstanceFunction GetFunction(string name) => null;

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
        return $"(GeneratedType) {Name}";
    }
}