using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class ElseIfExpression : Expression, IParsableNode, IChildExpression
{
    private readonly ExpressionList trueExpressions;

    public Expression Condition { get; }
    public IEnumerable<Expression> TrueExpressions => trueExpressions;

    private ElseIfExpression(Expression condition, ExpressionSource source, NodeReference parentReference, SourceReference reference) : base(source, parentReference, reference)
    {
        trueExpressions = new ExpressionList(this, reference);
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

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<IfExpression>("Not valid.");

        if (tokens.Length == 1) return ParseExpressionResult.Invalid<IfExpression>("No condition found");

        var expressionReference = new NodeReference();
        var condition = tokens.TokensFrom(1);
        var conditionExpression = ExpressionFactory.Parse(expressionReference, condition, source.Line);
        if (!conditionExpression.IsSuccess) return conditionExpression;

        var reference = source.CreateReference();

        var expression = new ElseIfExpression(conditionExpression.Result, source, parentReference, reference);
        expressionReference.SetNode(expression);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.ElseIf);
    }

    protected override void Validate(IValidationContext context)
    {
        var type = Condition.DeriveType(context);
        if (type == null || !type.Equals(ValueType.Boolean))
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

    public override Type DeriveType(IValidationContext context)
    {
        return null;
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return Condition.GetReadVariableUsage();
    }

    public override Symbol GetSymbol() => null;
}
