using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Expressions;

public class ExpressionList : Node, IReadOnlyList<Expression>
{
    private readonly List<Expression> values = new();

    public int Count => values.Count;
    public Expression this[int index] => values[index];

    public ExpressionList(INode parent, SourceReference reference) :
        base(new NodeReference(parent), reference)
    {
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
        context.InNodeVariableScope(this, base.ValidateTree);
    }

    public ParseExpressionResult Parse(IParseLineContext context)
    {
        var line = context.Line;
        var expression = ExpressionFactory.Parse(new NodeReference(this), line.Tokens, line);
        if (!expression.IsSuccess)
        {
            context.Logger.Fail(line.Tokens.AllReference(), expression.ErrorMessage);
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

    private void AddToParent(IChildExpression childExpression, IParseLineContext context)
    {
        var parentExpression = values.LastOrDefault() as IParentExpression;
        if (childExpression.ValidateParentExpression(parentExpression, context))
        {
            parentExpression.LinkChildExpression(childExpression);
        }
    }

    public override Symbol GetSymbol() => null;

    public override string ToString() => values.Count.ToString();
}
