using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.TypeSystem.Functions;
using Lexy.Compiler.Language.TypeSystem.Objects;

namespace Lexy.Compiler.Language.TypeSystem;

public class TableType : ObjectType
{
    public Table Table { get; }

    public TableType(Table table) : base(table.Name)
    {
        Table = table;
    }

    public override bool IsAssignableFrom(Type type) => Equals(type);

    protected bool Equals(TableType other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TableType)obj);
    }

    protected override IEnumerable<IObjectMember> CreateMembers()
    {
        var members = new List<IObjectMember>
        {
            new ObjectVariable(Table.RowsCountName, ValueType.Number),
            new ObjectNestedType(Table.RowName, Table.GetRowType()),
        };

        if (Table.Header?.Columns != null)
        {
            foreach (var column in Table.Header?.Columns)
            {
                var columnType = new GeneratedType(column.Name, Table, GeneratedTypeSource.TableColumn, Array.Empty<IObjectMember>());
                members.Add(new ObjectVariable(column.Name, columnType ));
            }
        }

        members.Add(new LookUpFunction(Table));
        members.Add(new LookUpRowFunction(Table));

        return members;
    }
}
