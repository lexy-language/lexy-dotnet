using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class CaseExpression : Expression, IParsableNode
{
    private readonly ExpressionList expressions;

    public Expression Value { get; }
    public IEnumerable<Expression> Expressions => expressions;
    public bool IsDefault { get; }

    private CaseExpression(Expression value, bool isDefault, ExpressionSource source, SourceReference reference,
        IExpressionFactory factory) : base(
        source, reference)
    {
        Value = value;
        IsDefault = isDefault;
        expressions = new ExpressionList(reference, factory);
    }

    public bool ValidatePreviousExpression(Expression expression, IParseLineContext context)
    {
        if (expression is SwitchExpression) return true;

        context.Logger.Fail(Reference,
            "'case' should be following a 'switch' statement. No 'switch' statement found.");
        return false;
    }

    public IParsableNode Parse(IParseLineContext context)
    {
        var expression = expressions.Parse(context);
        return expression.Result is IParsableNode node ? node : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (Value != null) yield return Value;

        yield return expressions;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<CaseExpression>("Not valid.");

        if (tokens.IsKeyword(0, Keywords.Default)) return ParseDefaultCase(source, tokens, factory);

        if (tokens.Length == 1)
            return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'case'. No parameters found.");

        var value = tokens.TokensFrom(1);
        var valueExpression = factory.Parse(value, source.Line);
        if (!valueExpression.IsSuccess) return valueExpression;

        var reference = source.CreateReference();

        var expression = new CaseExpression(valueExpression.Result, false, source, reference, factory);

        return ParseExpressionResult.Success(expression);
    }

    private static ParseExpressionResult ParseDefaultCase(ExpressionSource source, TokenList tokens,
        IExpressionFactory factory)
    {
        if (tokens.Length != 1)
            return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'default' case. No parameters expected.");

        var reference = source.CreateReference();
        var expression = new CaseExpression(null, true, source, reference, factory);
        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.IsKeyword(0, Keywords.Case)
               || tokens.IsKeyword(0, Keywords.Default);
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return Value?.DeriveType(context);
    }
}