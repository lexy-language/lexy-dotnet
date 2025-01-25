using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes;

public class TableType : TypeWithMembers
{
    public string TableName { get; }
    public Table Table { get; }

    public TableType(string tableName, Table table)
    {
        TableName = tableName;
        Table = table;
    }

    protected bool Equals(TableType other)
    {
        return TableName == other.TableName;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TableType)obj);
    }

    public override int GetHashCode()
    {
        return TableName != null ? TableName.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return TableName;
    }

    public override VariableType MemberType(string name, IValidationContext context)
    {
        return name switch
        {
            "Count" => PrimitiveType.Number,
            Table.RowName => TableRowType(context),
            _ => null
        };
    }

    private ComplexType TableRowType(IValidationContext context)
    {
        return context.RootNodes.GetTable(TableName)?.GetRowType(context);
    }
}