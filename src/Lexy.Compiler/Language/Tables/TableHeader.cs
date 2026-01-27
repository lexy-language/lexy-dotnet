using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Tables;

public class TableHeader : Node
{
    public IList<ColumnHeader> Columns { get; }

    private TableHeader(ColumnHeader[] columns, SourceReference reference) : base(reference)
    {
        Columns = Assert.NotNull(columns, nameof(columns));
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
        var index = 1;
        while (index < tokens.Length)
        {
            var header = ColumnHeader.Parse(context, index);
            if (header == null) return null;

            headers.Add(header);

            index += 3;
        }

        return new TableHeader(headers.ToArray(), context.Line.Tokens.AllReference());
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
}
