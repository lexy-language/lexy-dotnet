using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableRow : Node
{
    private readonly ValidationTableHeader tableHeader;

    public int Index { get; }
    public IList<ValidationTableValue> Values { get; }

    private ValidationTableRow(int index, ValidationTableHeader tableHeader, IList<ValidationTableValue> values,
        SourceReference reference) : base(reference)
    {
        Index = index;
        Values = Assert.NotNull(values, nameof(values));
        this.tableHeader = tableHeader;
    }

    public static ValidationTableRow Parse(IParseLineContext context, int index, ValidationTableHeader tableHeader)
    {
        var tokenIndex = 0;

        if (!context.ValidateTokens<ValidationTableRow>().Type<TableSeparatorToken>(tokenIndex).IsValid)
        {
            return null;
        }

        var values = new List<ValidationTableValue>();
        var currentLineTokens = context.Line.Tokens;
        while (++tokenIndex < currentLineTokens.Length)
        {
            var value = ParseValue(context, tableHeader, currentLineTokens, tokenIndex++, values.Count);
            if (value == null)
            {
                return null;
            }
            values.Add(value);
        }

        return new ValidationTableRow(index, tableHeader, values, context.Line.Tokens.AllReference());
    }

    private static ValidationTableValue ParseValue(IParseLineContext context,
        ValidationTableHeader tableHeader,
        TokenList currentLineTokens, int tokenIndex, int valueIndex)
    {
        var notValid = !context.ValidateTokens<ValidationTableRow>()
            .IsLiteralToken(tokenIndex)
            .Type<TableSeparatorToken>(tokenIndex + 1)
            .IsValid;

        if (notValid) return null;

        var reference = context.Line.Tokens.Reference(tokenIndex, 1);
        var token = currentLineTokens.Token<Token>(tokenIndex);
        var tokens = new TokenList(context.Line, new[] { token });
        var expression = context.ExpressionFactory.Parse(tokens, context.Line);

        return context.Failed(expression, reference)
            ? null
            : new ValidationTableValue(valueIndex, expression.Result, tableHeader, reference);
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
