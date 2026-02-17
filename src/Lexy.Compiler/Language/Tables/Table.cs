using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Tables;

public class Table : ComponentNode, INodeWithType
{
    private bool invalidHeader;
    private readonly List<TableRow> rows = new();

    public const string RowsCountName = "RowsCount";
    public const string RowName = "Row";

    public TableHeader Header { get; private set; }

    public IReadOnlyList<TableRow> Rows => rows;

    public Table(string name, NodeReference parentReference, SourceReference reference) :
        base(name, parentReference, reference)
    {
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
            Header = TableHeader.Parse(context, this);
            if (Header == null)
            {
                invalidHeader = true;
            }
        }
        else
        {
            var tableRow = TableRow.Parse(context, this);
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
        context.InNodeVariableScope(this, base.ValidateTree);
    }

    public GeneratedType GetRowType()
    {
        var members = Header?.Columns
            .Select(column => new ObjectVariable(column.Name, column.TypeDeclaration.Type))
            .ToList() ?? new List<ObjectVariable>();

        return new GeneratedType(Name, RowName, this, GeneratedTypeSource.TableRow, members);
    }

    public override Symbol GetSymbol()
    {
        if (Header == null) return null;

        var builder = new StringBuilder();
        foreach (var column in Header.Columns)
        {
            builder.AppendLine($"- {column.TypeDeclaration} {column.Name}");
        }
        var variablesString = builder.ToString();
        return new Symbol(Reference, "table: " + Name, variablesString, SymbolKind.Table);
    }
}
