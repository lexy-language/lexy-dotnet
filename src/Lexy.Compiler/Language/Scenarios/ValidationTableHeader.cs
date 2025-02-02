using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableHeader : Node
{
    public IList<ValidationColumnHeader> Columns { get; }

    private ValidationTableHeader(ValidationColumnHeader[] columns, SourceReference reference) : base(reference)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
    }

    public static ValidationTableHeader Parse(IParseLineContext context)
    {
        var startsWithTableSeparator = context.ValidateTokens<ValidationTableHeader>()
            .Type<TableSeparatorToken>(0)
            .IsValid;
        if (!startsWithTableSeparator) return null;

        return ParseWithoutColumnType(context);
    }

    private static ValidationTableHeader ParseWithoutColumnType(IParseLineContext context)
    {
        var headers = new List<ValidationColumnHeader>();
        var tokens = context.Line.Tokens;
        var index = 0;
        while (++index < tokens.Length)
        {
            var isValid = context.ValidateTokens<ValidationTableHeader>()
                .Type<TableSeparatorToken>(index + 1)
                .IsValid;

            if (!isValid) return null;

            var name = tokens.TokenValue(index);
            var reference = context.Line.TokenReference(index++);

            var header = ValidationColumnHeader.Parse(name, reference);
            headers.Add(header);
        }

        return new ValidationTableHeader(headers.ToArray(), context.Line.LineStartReference());
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Columns;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public ValidationColumnHeader GetColumn(int index)
    {
        return index >= 0 && index < Columns.Count ? Columns[index] : null;
    }
}