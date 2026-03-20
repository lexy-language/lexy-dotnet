using System.Collections.Generic;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem;

public class TableColumnType : Type, IHasNodeDependencies
{
    public IComponentNode Node { get;}
    public string TypeName { get; }
    public string MemberName { get; }
    public string Name { get; }

    public TableColumnType(string typeName, string memberName, IComponentNode node)
    {
        TypeName = typeName;
        MemberName = memberName;
        Name = $"{typeName}.{memberName}";
        Node = Assert.NotNull(node, nameof(node));
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        yield return Node;
    }

    public override bool IsAssignableFrom(Type type)
    {
        return type is TableColumnType tableColumnType
            && tableColumnType.Name == Name;
    }

    protected bool Equals(TableColumnType other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TableColumnType)obj);
    }

    public override int GetHashCode()
    {
        return (Name != null ? Name.GetHashCode() : 0);
    }

    public override Symbol GetSymbol(SourceReference reference)
    {
        return new Symbol(reference, $"table column: {Name}", string.Empty, SymbolKind.TableColumn);
    }

    public override string ToString() => Name;
}
