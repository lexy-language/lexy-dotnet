using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableValue : Node
{
    private readonly int index;
    private readonly ValidationTableHeader tableHeader;

    public Expression Expression { get; }

    public ValidationTableValue(int index, Expression expression, ValidationTableHeader tableHeader, SourceReference reference) : base(reference)
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
    }

    public object GetValue()
    {
        var literal = Expression as LiteralExpression;
        return literal?.Literal.TypedValue;
    }
}