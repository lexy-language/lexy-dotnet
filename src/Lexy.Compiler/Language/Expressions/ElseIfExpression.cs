using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class ElseIfExpression : Expression, IParsableNode, IChildExpression
{
    private readonly ExpressionList trueExpressions;

    public Expression Condition { get; }
    public IEnumerable<Expression> TrueExpressions => trueExpressions;

    private ElseIfExpression(Expression condition, ExpressionSource source, SourceReference reference,
        IExpressionFactory factory) : base(source, reference)
    {
        trueExpressions = new ExpressionList(reference, factory);
        Condition = condition;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Condition;
        yield return trueExpressions;
    }

    public IParsableNode Parse(IParseLineContext context)
    {
        var expression = trueExpressions.Parse(context);
        return expression.Result is IParsableNode node ? node : this;
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

        var expression = new ElseIfExpression(conditionExpression.Result, source, reference, factory);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.ElseIf);
    }

    protected override void Validate(IValidationContext context)
    {
        var type = Condition.DeriveType(context);
        if (type == null || !type.Equals(PrimitiveType.Boolean))
        {
            context.Logger.Fail(Reference,
                $"'elseif' condition expression should be 'boolean', is of wrong type '{type}'.");
        }
    }

    public bool ValidateParentExpression(IParentExpression expression, IParseLineContext context)
    {
        if (expression is IfExpression) return true;
        context.Logger.Fail(Reference, "'elseif' should be following an 'if' statement. No 'if' statement found.");

        return false;
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