using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class IfExpression : Expression, IParsableNode, IParentExpression
{
    private readonly ExpressionList trueExpressions;
    private readonly List<Expression> elseExpressions = new List<Expression>();

    public Expression Condition { get; }
    public IEnumerable<Expression> TrueExpressions => trueExpressions;

    public IReadOnlyList<Expression> ElseExpressions => elseExpressions;

    private IfExpression(Expression condition, ExpressionSource source, SourceReference reference, IExpressionFactory factory) : base(source,
        reference)
    {
        Condition = condition;
        trueExpressions = new ExpressionList(reference, factory);
    }

    public IParsableNode Parse(IParseLineContext context)
    {
        var expression = trueExpressions.Parse(context);
        return expression.Result is IParsableNode node ? node : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Condition;
        yield return trueExpressions;
        foreach (var elseExpression in elseExpressions)
        {
            yield return elseExpression;
        }
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<IfExpression>("Not valid.");

        if (tokens.Length == 1) return ParseExpressionResult.Invalid<IfExpression>("No condition found");

        var condition = tokens.TokensFrom(1);
        var conditionExpression = factory.Parse(condition, source.Line);
        if (!conditionExpression.IsSuccess) return conditionExpression;

        var reference = source.CreateReference();

        var expression = new IfExpression(conditionExpression.Result, source, reference, factory);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.If);
    }

    protected override void Validate(IValidationContext context)
    {
        var type = Condition.DeriveType(context);
        if (type == null || !type.Equals(PrimitiveType.Boolean))
        {
            context.Logger.Fail(Reference,
                $"'if' condition expression should be 'boolean', is of wrong type '{type}'.");
        }
    }

    public void LinkChildExpression(IChildExpression expression)
    {
        Assert.NotNull(expression, nameof(expression));

        if (expression is not (ElseExpression or ElseIfExpression))
        {
            throw new InvalidOperationException($"Invalid node type: {expression.GetType().Name}");
        }

        var lastOrDefaultExpression = elseExpressions.LastOrDefault();
        if (lastOrDefaultExpression is ElseExpression)
        {
            throw new InvalidOperationException("'else' already defined.");
        }
        elseExpressions.Add((Expression)expression);
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return null;
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return Condition.GetReadVariableUsage();
    }
}