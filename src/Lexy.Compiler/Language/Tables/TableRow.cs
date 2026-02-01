using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Tables;

public class TableRow : Node
{
    private readonly TableHeader tableHeader;

    public IList<TableValue> Values { get; }

    private TableRow(Table table, IList<TableValue> values, SourceReference reference) : base(table, reference)
    {
        Values = Assert.NotNull(values, nameof(values));
        tableHeader = table.Header;
    }

    public static TableRow Parse(IParseLineContext context, TableHeader tableHeader, Table table)
    {
        var tokenIndex = 0;

        if (!context.ValidateTokens<TableRow>().Type<TableSeparatorToken>(tokenIndex).IsValid)
        {
            return null;
        }

        var rowReference = new NodeReference();
        var values = new List<TableValue>();
        var currentLineTokens = context.Line.Tokens;
        while (++tokenIndex < currentLineTokens.Length)
        {
            var value = TableValue.Parse(context, tableHeader, currentLineTokens, rowReference, tokenIndex++, values.Count);
            if (value == null)
            {
                return null;
            }
            values.Add(value);
        }

        var tableRow = new TableRow(table, values, context.Line.Tokens.AllReference());
        rowReference.SetNode(tableRow);
        return tableRow;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Values.ToList();
    }

    protected override void Validate(IValidationContext context)
    {
        if (tableHeader.Columns.Count != Values.Count)
        {
            context.Logger.Fail(Reference,
                $"Invalid number of values {Values.Count}. Expected {tableHeader.Columns.Count}.");
        }
    }

    public override Symbol GetSymbol() => null;
}
