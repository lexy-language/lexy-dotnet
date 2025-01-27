using System.Collections.Generic;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class Table : ParsableNode
{
    private bool invalidHeader;

    public TableHeader Header { get; private set; }
    public IList<TableRow> Rows { get; } = new List<TableRow>();

    public Table(SourceReference reference) : base(reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        if (invalidHeader) return this;

        if (Header == null)
        {
            Header = TableHeader.Parse(context);
            if (Header == null)
            {
                invalidHeader = true;
            }
            return this;
        }

        var row = TableRow.Parse(context, Header);
        if (row != null) Rows.Add(row);

        return this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (Header != null) yield return Header;

        foreach (var row in Rows) yield return row;
    }

    protected override void Validate(IValidationContext context)
    {
    }
}