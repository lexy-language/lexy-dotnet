using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

internal class ExpressionList : Node, IReadOnlyList<Expression>
{
    private readonly List<Expression> values = new();

    public ExpressionList(SourceReference reference) : base(reference)
    {
    }

    public int Count => values.Count;
    public Expression this[int index] => values[index];

    public IEnumerator<Expression> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Expression expression, IParserContext context)
    {
        if (expression is IDependantExpression childExpression)
            childExpression.LinkPreviousExpression(values.LastOrDefault(), context);
        else
            values.Add(expression);
    }

    public override IEnumerable<INode> GetChildren()
    {
        return values;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override void ValidateTree(IValidationContext context)
    {
        using (context.CreateVariableScope())
        {
            base.ValidateTree(context);
        }
    }
}