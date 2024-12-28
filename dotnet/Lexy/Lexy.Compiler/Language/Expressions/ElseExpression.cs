using System.Collections.Generic;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions;

public class ElseExpression : Expression, IParsableNode, IDependantExpression
{
    private readonly ExpressionList falseExpressions;

    public IEnumerable<Expression> FalseExpressions => falseExpressions;

    private ElseExpression(ExpressionSource source, SourceReference reference) : base(source, reference)
    {
        falseExpressions = new ExpressionList(reference);
    }

    public void LinkPreviousExpression(Expression expression, IParserContext context)
    {
        if (!(expression is IfExpression ifExpression))
        {
            context.Logger.Fail(Reference, "Else should be following an If statement. No if statement found.");
            return;
        }

        ifExpression.LinkElse(this);
    }

    public override IEnumerable<INode> GetChildren()
    {
        foreach (var expression in FalseExpressions) yield return expression;
    }

    public IParsableNode Parse(IParserContext context)
    {
        var line = context.CurrentLine;
        if (line.IsComment() || line.IsEmpty()) return this;

        var expression = ExpressionFactory.Parse(context.SourceCode.File, line.Tokens, line);
        if (!expression.IsSuccess)
        {
            context.Logger.Fail(context.LineStartReference(), expression.ErrorMessage);
            return null;
        }

        falseExpressions.Add(expression.Result, context);
        return expression.Result is IParsableNode node ? node : this;
    }

    public static ParseExpressionResult Parse(ExpressionSource source)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ElseExpression>("Not valid.");

        if (tokens.Length > 1) return ParseExpressionResult.Invalid<ElseExpression>("No tokens expected.");

        var reference = source.CreateReference();

        var expression = new ElseExpression(source, reference);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.Else);
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return null;
    }
}