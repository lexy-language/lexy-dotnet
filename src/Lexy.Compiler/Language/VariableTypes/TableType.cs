using System;
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
        switch(name)
        {
            case "Count":
                return PrimitiveType.Number;
            case Table.RowName:
                return TableRowType(context);
        };
        if (Table.Header?.GetColumn(name) != null) return new ComplexType(name, Table, ComplexTypeSource.TableColumn, Array.Empty<ComplexTypeMember>());
        return null;
    }

    private ComplexType TableRowType(IValidationContext context)
    {
        return context.RootNodes.GetTable(TableName)?.GetRowType();
    }
}