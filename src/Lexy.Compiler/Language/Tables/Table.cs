using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class Table : ComponentNode
{
    private bool invalidHeader;
    private readonly List<TableRow> rows = new();

    public const string CountName = "Count";
    public const string RowName = "Row";

    public TableName Name { get; } = new();
    public TableHeader Header { get; private set; }

    public IReadOnlyList<TableRow> Rows => rows;

    public override string NodeName => Name.Value;

    public Table(string name, SourceReference reference) : base(reference)
    {
        Name.ParseName(name);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        if (invalidHeader) return this;

        if (IsFirstLine())
        {
            Header = TableHeader.Parse(context);
            if (Header == null)
            {
                invalidHeader = true;
            }
        }
        else
        {
            var tableRow = TableRow.Parse(context, this.Header);
            if (tableRow != null) rows.Add(tableRow);
        }

        return this;
    }

    private bool IsFirstLine()
    {
        return Header == null;
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (Header != null) yield return Header;

        foreach (var row in Rows) yield return row;
    }

    protected override void Validate(IValidationContext context)
    {
        if (Header == null)
        {
            context.Logger.Fail(Reference, "No table header found.");
        }
    }

    public override void ValidateTree(IValidationContext context)
    {
        using (context.CreateVariableScope())
        {
            base.ValidateTree(context);
        }
    }

    public ComplexType GetRowType()
    {
        var members = Header?.Columns
            .Select(column => new ComplexTypeMember(column.Name, column.Type.VariableType))
            .ToList() ?? new List<ComplexTypeMember>();

        return new ComplexType(Name.Value, this, ComplexTypeSource.TableRow, members);
    }
}