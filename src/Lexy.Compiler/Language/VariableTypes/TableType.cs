using System;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.VariableTypes.Functions;

namespace Lexy.Compiler.Language.VariableTypes;

public class TableType : ObjectType
{
    public string TableName { get; }
    public Table Table { get; }

    public TableType(string tableName, Table table)
    {
        TableName = tableName;
        Table = table;
    }

    public override bool IsAssignableFrom(VariableType type) => Equals(type);

    public override VariableType MemberType(string name, IComponentNodeList componentNodes)
    {
        return name switch
        {
            Table.CountName => PrimitiveType.Number,
            Table.RowName => TableRowType(componentNodes),
            _ => Table.Header?.GetColumn(name) != null
                ? new GeneratedType(name, Table, GeneratedTypeSource.TableColumn, Array.Empty<ObjectTypeVariable>())
                : null
        };
    }

    public override IObjectTypeVariable GetVariable(string name) => null;

    public override IObjectTypeFunction GetFunction(string name)
    {
        return name switch
        {
            LookUpFunction.Name => new LookUpFunction(Table),
            LookUpRowFunction.Name => new LookUpRowFunction(Table),
            _ => null
        };
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


    private GeneratedType TableRowType(IComponentNodeList componentNodes)
    {
        return componentNodes.GetTable(TableName)?.GetRowType();
    }
}