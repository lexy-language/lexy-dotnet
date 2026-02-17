using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

namespace Lexy.Compiler.Language.Expressions;

public class IfExpression : Expression, IParsableNode, IParentExpression
{
    private readonly ExpressionList trueExpressions;
    private readonly List<Expression> elseExpressions = new ();

    public Expression Condition { get; }
    public IReadOnlyList<Expression> TrueExpressions => trueExpressions;

    public IReadOnlyList<Expression> ElseExpressions => elseExpressions;

    private IfExpression(Expression condition, ExpressionSource source, NodeReference parentReference, SourceReference reference, IExpressionFactory factory) :
        base(source, parentReference, reference)
    {
        Condition = condition;
        trueExpressions = new ExpressionList(this, reference, factory);
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

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<IfExpression>("Not valid.");

        if (tokens.Length == 1) return ParseExpressionResult.Invalid<IfExpression>("No condition found");

        var expressionReference = new NodeReference();
        var condition = tokens.TokensFrom(1);
        var conditionExpression = factory.Parse(expressionReference, condition, source.Line);
        if (!conditionExpression.IsSuccess) return conditionExpression;

        var reference = source.CreateReference();

        var expression = new IfExpression(conditionExpression.Result, source, parentReference, reference, factory);
        expressionReference.SetNode(expression);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.If);
    }

    protected override void Validate(IValidationContext context)
    {
        var type = Condition.DeriveType(context);
        if (type == null || !type.Equals(ValueType.Boolean))
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

    public override Type DeriveType(IValidationContext context)
    {
        return null;
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return Condition.GetReadVariableUsage();
    }

    public override Symbol GetSymbol() => null;

    public override SuggestionEdit[] GetSuggestions()
    {
        return Suggestions.Edit(with => with
            .Keyword(Keywords.Else)
            .Keyword(Keywords.ElseIf)
        );
    }
}
