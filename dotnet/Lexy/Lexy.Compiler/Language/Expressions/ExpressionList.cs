using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

internal class ExpressionList : Node, IReadOnlyList<Expression>
{
    private readonly IExpressionFactory factory;
    private readonly List<Expression> values = new();

    public int Count => values.Count;
    public Expression this[int index] => values[index];

    public ExpressionList(SourceReference reference, IExpressionFactory factory) : base(reference)
    {
        this.factory = factory;
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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

    public ParseExpressionResult Parse(IParseLineContext context)
    {
        var line = context.Line;
        var expression = factory.Parse(line.Tokens, line);
        if (!expression.IsSuccess)
        {
            context.Logger.Fail(line.LineStartReference(), expression.ErrorMessage);
            return expression;
        }

        Add(expression.Result, context);
        return expression;
    }

    private void Add(Expression expression, IParseLineContext context)
    {
        if (expression is not IChildExpression childExpression)
        {
            values.Add(expression);
        }
        else
        {
            AddToParent(childExpression, context);
        }
    }

    private void AddToParent(IChildExpression childExpression, IParseLineContext context) {
        var parentExpression = values.LastOrDefault() as IParentExpression;
        if (childExpression.ValidatePreviousExpression(parentExpression, context)) {
            parentExpression.LinkChildExpression(childExpression);
        }
    }
}