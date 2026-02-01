using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableValue : Node
{
    public Expression Expression { get; }

    public ValidationTableValue(Expression expression, NodeReference parentReference, SourceReference reference) :
        base(parentReference, reference)
    {
        Expression = expression;
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
        if (Expression is MemberAccessExpression memberAccessExpression)
        {
            return memberAccessExpression.IdentifierPath.ToString();
        }

        var literal = Expression as LiteralExpression;
        return literal?.Literal.TypedValue;
    }

    public override Symbol GetSymbol() => null;
}
