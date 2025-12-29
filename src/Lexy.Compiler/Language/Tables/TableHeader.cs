using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Tables;

public class TableHeader : Node
{
    public IList<ColumnHeader> Columns { get; }

    private TableHeader(ColumnHeader[] columns, SourceReference reference) : base(reference)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
    }

    public static TableHeader Parse(IParseLineContext context)
    {
        var startsWithTableSeparator = context.ValidateTokens<TableHeader>()
            .Type<TableSeparatorToken>(0).IsValid;

        if (!startsWithTableSeparator) return null;

        return ParseWithColumnType(context);
    }

    private static TableHeader ParseWithColumnType(IParseLineContext context)
    {
        var headers = new List<ColumnHeader>();
        var tokens = context.Line.Tokens;
        var index = 0;
        while (++index < tokens.Length)
        {
            var isValid = context.ValidateTokens<TableHeader>()
                .Type<StringLiteralToken>(index)
                .Type<StringLiteralToken>(index + 1)
                .Type<TableSeparatorToken>(index + 2)
                .IsValid;

            if (!isValid) return null;

            var typeName = tokens.TokenValue(index);
            var name = tokens.TokenValue(++index);
            var reference = context.Line.TokenReference(index);

            var header = ColumnHeader.Parse(name, typeName, reference);
            headers.Add(header);

            ++index;
        }

        return new TableHeader(headers.ToArray(), context.Line.LineStartReference());
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
}