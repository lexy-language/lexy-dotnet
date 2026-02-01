using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class LiteralExpression : Expression
{
    public ILiteralToken Literal { get; }

    private LiteralExpression(ILiteralToken literal, ExpressionSource source, NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        Literal = Assert.NotNull(literal, nameof(literal));
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var expression = CreateExpression(parentReference, source, source.Tokens);
        return expression == null
             ? ParseExpressionResult.Invalid<LiteralExpression>("Invalid expression.")
             : ParseExpressionResult.Success(expression);
    }

    public static ParseLiteralExpressionResult ParseLiteral(NodeReference parentReference, ExpressionSource source, IExpressionFactory factory)
    {
        var expression = CreateExpression(parentReference, source, source.Tokens);
        return expression == null
             ? ParseLiteralExpressionResult.Invalid<LiteralExpression>("Invalid expression.")
             : ParseLiteralExpressionResult.Success(expression);
    }

    private static LiteralExpression CreateExpression(NodeReference parentReference, ExpressionSource source, TokenList tokens)
    {
        if (!IsValid(source.Tokens)) return null;

        var reference = source.CreateReference();

        if (tokens.Length == 2) return NegativeNumeric(parentReference, source, tokens, reference);

        var literalToken = tokens.LiteralToken(0);
        return new LiteralExpression(literalToken, source, parentReference, reference);
    }

    private static LiteralExpression NegativeNumeric(NodeReference parentReference, ExpressionSource source, TokenList tokens,
        SourceReference reference)
    {
        var operatorToken = tokens.OperatorToken(0);
        var valueToken = tokens.LiteralToken(1);
        if (valueToken is not NumberLiteralToken numericLiteralToken)
        {
            throw new InvalidOperationException($"{valueToken.GetType()} should be NumberLiteralToken");
        }

        var value = -numericLiteralToken.NumberValue;
        var negatedLiteral = new NumberLiteralToken(value, operatorToken.FirstCharacter);

        return new LiteralExpression(negatedLiteral, source, parentReference, reference);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1
            && tokens.IsLiteralToken(0)
            || tokens.Length == 2
            && tokens.IsOperatorToken(0, OperatorType.Subtraction)
            && tokens.IsLiteralToken(1)
            && tokens.LiteralToken(1) is NumberLiteralToken;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Type DeriveType(IValidationContext context)
    {
        return Literal.DeriveType(context);
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, Literal.ToString(), string.Empty, SymbolKind.Constant);
    }

    public override string ToString() => Literal?.Value;
}
