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
            if (ParseValue(context, tableHeader, currentLineTokens, values, tokenIndex++))
            {
                return null;
            }
        }

        return new TableRow(tableHeader, values, context.Line.LineStartReference());
    }

    private static bool ParseValue(IParseLineContext context, TableHeader tableHeader,
        TokenList currentLineTokens, List<TableValue> values, int tokenIndex)
    {
        var notValid = !context.ValidateTokens<TableRow>()
            .IsLiteralToken(tokenIndex)
            .Type<TableSeparatorToken>(tokenIndex + 1)
            .IsValid;

        if (notValid) return true;

        var reference = context.Line.TokenReference(tokenIndex);
        var token = currentLineTokens.Token<Token>(tokenIndex);
        var expression = context.ExpressionFactory.Parse(new TokenList(new[] { token }), context.Line);
        if (context.Failed(expression, reference)) return true;

        values.Add(new TableValue(values.Count, expression.Result, tableHeader, reference));
        return false;
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