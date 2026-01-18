using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class Table : ComponentNode, INodeWithType
{
    private bool invalidHeader;
    private readonly List<TableRow> rows = new();

    public const string RowsCountName = "RowsCount";
    public const string RowName = "Row";

    public TableHeader Header { get; private set; }

    public IReadOnlyList<TableRow> Rows => rows;

    public override string Name { get; }

    public Table(string name, SourceReference reference) : base(reference)
    {
        Name = name;
    }

    public Type CreateType()
    {
        return new TableType(this);
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

    public GeneratedType GetRowType()
    {
        var members = Header?.Columns
            .Select(column => new ObjectVariable(column.Name, column.TypeDeclaration.Type))
            .ToList() ?? new List<ObjectVariable>();

        return new GeneratedType(Name, this, GeneratedTypeSource.TableRow, members);
    }
}
