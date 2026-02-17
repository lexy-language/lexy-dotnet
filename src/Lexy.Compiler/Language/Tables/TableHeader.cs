using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Tables;

public class TableHeader : Node
{
    public IList<ColumnHeader> Columns { get; }

    private TableHeader(ColumnHeader[] columns, Table table, SourceReference reference) : base(table, reference)
    {
        Columns = columns;
    }

    public static TableHeader Parse(IParseLineContext context, Table table)
    {
        var startsWithTableSeparator = context.ValidateTokens<TableHeader>()
            .Type<TableSeparatorToken>(0).IsValid;

        if (!startsWithTableSeparator) return null;

        return ParseWithColumnType(context, table);
    }

    private static TableHeader ParseWithColumnType(IParseLineContext context, Table table)
    {
        var headerReference = new NodeReference();
        var headers = new List<ColumnHeader>();
        var tokens = context.Line.Tokens;
        var index = 1;
        while (index < tokens.Length)
        {
            var header = ColumnHeader.Parse(context, headerReference, index);
            if (header == null) return null;

            headers.Add(header);

            index += 3;
        }

        var tableHeader = new TableHeader(headers.ToArray(), table, context.Line.Tokens.AllReference());
        headerReference.SetNode(tableHeader);
        return tableHeader;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Columns;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public ColumnHeader Get(IdentifierPath path)
    {
        if (path.Parts < 2) return null;
        var name = path.Path[1];

        return GetColumn(name);
    }

    public ColumnHeader GetColumn(int index)
    {
        return index >= 0 && index < Columns.Count ? Columns[index] : null;
    }

    public ColumnHeader GetColumn(string name)
    {
        return Columns.FirstOrDefault(value => value.Name == name);
    }

    public override Symbol GetSymbol() => null;

    public override string ToString() => Columns.Count.ToString();
}
