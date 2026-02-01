using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTable : ParsableNode, INodeWithName
{
    private bool invalidHeader;
    private readonly List<ValidationTableRow> rows = new();

    public string Name { get; }
    public ValidationTableHeader Header { get; private set; }

    public IReadOnlyList<ValidationTableRow> Rows => rows;

    public ValidationTable(string name, Scenario parent, SourceReference reference) :
        base(new NodeReference(parent), reference)
    {
        Name = name;
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        if (invalidHeader) return this;

        if (IsFirstLine())
        {
            Header = ValidationTableHeader.Parse(context, this);
            if (Header == null)
            {
                invalidHeader = true;
            }
        }
        else
        {
            var tableRow = ValidationTableRow.Parse(context, rows.Count, Header, this);
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
        context.InNodeVariableScope(this, base.ValidateTree);
    }

    public override Symbol GetSymbol() => null;
}
