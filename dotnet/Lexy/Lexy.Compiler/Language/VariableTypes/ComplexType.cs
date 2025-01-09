using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public class ComplexType : VariableType, ITypeWithMembers
{
    public string Name { get; }
    public ComplexTypeSource Source { get; }
    public IEnumerable<ComplexTypeMember> Members { get; }
    public IRootNode Node { get;}

    public ComplexType(string name, IRootNode node, ComplexTypeSource source, IEnumerable<ComplexTypeMember> members)
    {
        Name = name;
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Source = source;
        Members = members ?? throw new ArgumentNullException(nameof(members));
    }

    public VariableType MemberType(string name, IValidationContext context)
    {
        return Members.FirstOrDefault(member => member.Name == name)?.Type;
    }

    protected bool Equals(ComplexType other)
    {
        return Name == other.Name && Source == other.Source;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ComplexType)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Source);
    }
}