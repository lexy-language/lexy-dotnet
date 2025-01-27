using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Tables;

public class TableValue : Node
{
    private readonly int index;
    private readonly TableHeader tableHeader;

    public Expression Expression { get; }

    public TableValue(int index, Expression expression, TableHeader tableHeader, SourceReference reference) : base(reference)
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
        var expectedType = column.Type.VariableType;
        if (expectedType?.Equals(actualType) != true)
        {
            context.Logger.Fail(Reference, $"Invalid value type '{actualType}'. Expected '{expectedType}'.");
        }
    }
}