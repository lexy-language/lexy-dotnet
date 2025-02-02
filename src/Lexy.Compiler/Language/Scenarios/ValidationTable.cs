using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTable : ParsableNode
{
    private bool invalidHeader;
    private readonly List<ValidationTableRow> rows = new();

    public ValidationTableName Name { get; } = new();
    public ValidationTableHeader Header { get; private set; }

    public IReadOnlyList<ValidationTableRow> Rows => rows;

    public ValidationTable(string name, SourceReference reference) : base(reference)
    {
        Name.ParseName(name);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        if (invalidHeader) return this;

        if (IsFirstLine())
        {
            Header = ValidationTableHeader.Parse(context);
            if (Header == null)
            {
                invalidHeader = true;
            }
        }
        else
        {
            var tableRow = ValidationTableRow.Parse(context, rows.Count, Header);
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

        foreach (var row in Rows)
        {
            yield return row;
        }
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
}