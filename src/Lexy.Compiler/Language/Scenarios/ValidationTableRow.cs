using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableRow : Node
{
    private readonly ValidationTableHeader tableHeader;

    public int Index { get; }
    public IList<ValidationTableValue> Values { get; }

    private ValidationTableRow(int index, ValidationTableHeader tableHeader, IList<ValidationTableValue> values,
        ValidationTable validationTable, SourceReference reference) :
        base(new NodeReference(validationTable), reference)
    {
        Index = index;
        Values = Assert.NotNull(values, nameof(values));
        this.tableHeader = tableHeader;
    }

    public static ValidationTableRow Parse(IParseLineContext context, int index,
        ValidationTableHeader tableHeader, ValidationTable validationTable)
    {
        var tokenIndex = 0;

        if (!context.ValidateTokens<ValidationTableRow>().Type<TableSeparatorToken>(tokenIndex).IsValid)
        {
            return null;
        }

        var tableRowReference = new NodeReference();
        var values = new List<ValidationTableValue>();
        var currentLineTokens = context.Line.Tokens;
        while (++tokenIndex < currentLineTokens.Length)
        {
            var value = ParseValue(context, values.Count, tableRowReference, currentLineTokens, tokenIndex++);
            if (value == null)
            {
                return null;
            }
            values.Add(value);
        }

        var validationTableRow = new ValidationTableRow(index, tableHeader, values, validationTable, context.Line.Tokens.AllReference());
        tableRowReference.SetNode(validationTableRow);

        return validationTableRow;
    }

    private static ValidationTableValue ParseValue(IParseLineContext context,
        int index,
        NodeReference tableRowReference,
        TokenList currentLineTokens, int tokenIndex)
    {
        var notValid = !context.ValidateTokens<ValidationTableRow>()
            .IsLiteralToken(tokenIndex)
            .Type<TableSeparatorToken>(tokenIndex + 1)
            .IsValid;

        if (notValid) return null;

        var reference = context.Line.Tokens.Reference(tokenIndex, 1);
        var token = currentLineTokens.Token<Token>(tokenIndex);
        var tokens = new TokenList(context.Line, new[] { token });
        var tableValueReference = new NodeReference();
        var expression = ExpressionFactory.Parse(tableValueReference, tokens, context.Line);

        if (context.Failed(expression, reference)) return null;

        var validationTableValue = new ValidationTableValue(index, expression.Result, tableRowReference, reference);
        tableValueReference.SetNode(validationTableValue);
        return validationTableValue;
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

    public override string ToString()
    {
        return $"[{Index}]";
    }
}
