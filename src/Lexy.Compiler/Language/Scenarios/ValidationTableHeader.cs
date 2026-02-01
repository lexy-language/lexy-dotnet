using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableHeader : Node
{
    public IList<ValidationColumnHeader> Columns { get; }

    private ValidationTableHeader(ValidationColumnHeader[] columns, ValidationTable parent, SourceReference reference) :
        base(new NodeReference(parent), reference)
    {
        Columns = Assert.NotNull(columns, nameof(columns));
    }

    public static ValidationTableHeader Parse(IParseLineContext context, ValidationTable validationTable)
    {
        var startsWithTableSeparator = context.ValidateTokens<ValidationTableHeader>()
            .Type<TableSeparatorToken>(0)
            .IsValid;
        if (!startsWithTableSeparator) return null;

        return ParseWithoutColumnType(context, validationTable);
    }

    private static ValidationTableHeader ParseWithoutColumnType(IParseLineContext context, ValidationTable validationTable)
    {
        var headerReference = new NodeReference();
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
            var reference = context.Line.Tokens.Reference(index++, 1);

            var header = ValidationColumnHeader.Parse(name, headerReference, reference);
            headers.Add(header);
        }

        var validationTableHeader = new ValidationTableHeader(headers.ToArray(), validationTable, context.Line.Tokens.AllReference());
        headerReference.SetNode(validationTableHeader);
        return validationTableHeader;
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

    public override Symbol GetSymbol() => null;
}
