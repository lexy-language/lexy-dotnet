using System;
using System.Collections.Generic;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public class GeneratedType : ObjectType
{
    public GeneratedTypeSource Source { get; }
    public IComponentNode Node { get;}

    public GeneratedType(string name, IComponentNode node, GeneratedTypeSource source, IEnumerable<IObjectMember> members) :
        base(name, members)
    {
        Node = Assert.NotNull(node, nameof(node));
        Source = source;
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

    public override IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return Node;
    }
}
