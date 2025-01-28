using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Tables;

public class TableRow : Node
{
    private readonly TableHeader tableHeader;
    public IList<TableValue> Values { get; }

    private TableRow(TableHeader tableHeader, IList<TableValue> values, SourceReference reference) : base(reference)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
        this.tableHeader = tableHeader;
    }

    public static TableRow Parse(IParseLineContext context, TableHeader tableHeader)
    {
        var tokenIndex = 0;

        if (!context.ValidateTokens<TableRow>().Type<TableSeparatorToken>(tokenIndex).IsValid)
        {
            return null;
        }

        var values = new List<TableValue>();
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

        return new TableRow(tableHeader, values, context.Line.LineStartReference());
    }

    private static TableValue ParseValue(IParseLineContext context, TableHeader tableHeader,
        TokenList currentLineTokens, int tokenIndex, int valueIndex)
    {
        var notValid = !context.ValidateTokens<TableRow>()
            .IsLiteralToken(tokenIndex)
            .Type<TableSeparatorToken>(tokenIndex + 1)
            .IsValid;

        if (notValid) return null;

        var reference = context.Line.TokenReference(tokenIndex);
        var token = currentLineTokens.Token<Token>(tokenIndex);
        var expression = context.ExpressionFactory.Parse(new TokenList(new[] { token }), context.Line);
        if (context.Failed(expression, reference)) return null;

        return new TableValue(valueIndex, expression.Result, tableHeader, reference);
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
}