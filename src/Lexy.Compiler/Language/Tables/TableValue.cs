using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Tables;

public class TableValue : Node
{
    private readonly int index;
    private readonly TableHeader tableHeader;

    public Expression Expression { get; }

    private TableValue(int index, Expression expression, TableHeader tableHeader, NodeReference parentReference, SourceReference reference) :
        base(parentReference, reference)
    {
        Expression = expression;
        this.index = index;
        this.tableHeader = tableHeader;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Expression;
    }

    protected override void Validate(IValidationContext context)
    {
        var column = tableHeader.GetColumn(index);
        if (column == null) return;

        var actualType = Expression.DeriveType(context);
        var expectedType = column.TypeDeclaration.Type;
        if (expectedType?.Equals(actualType) != true)
        {
            context.Logger.Fail(Reference, $"Invalid value type '{actualType}'. Expected '{expectedType}'.");
        }
    }

    public override Symbol GetSymbol() => null;

    public static TableValue Parse(IParseLineContext context, TableHeader tableHeader,
        TokenList currentLineTokens, NodeReference parentReference, int tokenIndex, int valueIndex)
    {
        var notValid = !context.ValidateTokens<TableRow>()
            .IsLiteralToken(tokenIndex)
            .Type<TableSeparatorToken>(tokenIndex + 1)
            .IsValid;

        if (notValid) return null;

        var valueReference = new NodeReference();
        var reference = context.Line.Tokens.Reference(tokenIndex, 1);
        var token = currentLineTokens.Token<Token>(tokenIndex);
        var expression = context.ExpressionFactory.Parse(valueReference, new TokenList(context.Line, token), context.Line);
        if (context.Failed(expression, reference)) return null;

        var tableValue = new TableValue(valueIndex, expression.Result, tableHeader, parentReference, reference);
        valueReference.SetNode(tableValue);
        return tableValue;
    }

    public override string ToString() => $"[{index}]";
}
