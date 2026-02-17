using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class CaseExpression : Expression, IParsableNode
{
    private readonly ExpressionList expressions;

    public Expression Value { get; }
    public IReadOnlyList<Expression> Expressions => expressions;
    public bool IsDefault { get; }

    private CaseExpression(Expression value, bool isDefault, ExpressionSource source,
        NodeReference parentReference, SourceReference reference,
        IExpressionFactory factory) : base(source, parentReference, reference)
    {
        Value = value;
        IsDefault = isDefault;
        expressions = new ExpressionList(this, reference, factory);
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

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<CaseExpression>("Not valid.");

        if (tokens.IsKeyword(0, Keywords.Default))
        {
            return ParseDefaultCase(parentReference, source, tokens, factory);
        }

        if (tokens.Length == 1)
        {
            return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'case'. No parameters found.");
        }

        var expressionReference = new NodeReference();
        var value = tokens.TokensFrom(1);
        var valueExpression = factory.Parse(expressionReference, value, source.Line);
        if (!valueExpression.IsSuccess) return valueExpression;

        var reference = source.CreateReference();

        var expression = new CaseExpression(valueExpression.Result, false, source, parentReference, reference, factory);
        expressionReference.SetNode(expression);
        return ParseExpressionResult.Success(expression);
    }

    private static ParseExpressionResult ParseDefaultCase(NodeReference parentReference, ExpressionSource source, TokenList tokens,
        IExpressionFactory factory)
    {
        if (tokens.Length != 1)
        {
            return ParseExpressionResult.Invalid<CaseExpression>("Invalid 'default' case. No parameters expected.");
        }

        var reference = source.CreateReference();
        var expression = new CaseExpression(null, true, source, parentReference, reference, factory);
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

    public override Type DeriveType(IValidationContext context)
    {
        return Value?.DeriveType(context);
    }

    public override Symbol GetSymbol() => null;
}
