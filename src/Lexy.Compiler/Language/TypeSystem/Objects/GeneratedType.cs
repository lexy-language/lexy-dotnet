using System;
using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Objects;

public class GeneratedType : ObjectType
{
    public GeneratedTypeSource Source { get; }
    public IComponentNode Node { get;}
    public string TypeName { get; set; }
    public string MemberName { get; set; }

    public GeneratedType(string typeName, string memberName, IComponentNode node, GeneratedTypeSource source, IEnumerable<IObjectMember> members) :
        base($"{typeName}.{memberName}", members)
    {
        TypeName = typeName;
        MemberName = memberName;
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

    public override string ToString()
    {
        return Name;
    }

    public override Symbol GetSymbol(SourceReference reference)
    {
        return new Symbol(reference, $"type: {Name}", string.Empty, SymbolKind.GeneratedType);
    }
}
