using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class BracketedExpression : Expression
{
    public string FunctionName { get; }

    public Expression Expression { get; }

    private BracketedExpression(string functionName, Expression expression, NodeReference parentReference, ExpressionSource source, SourceReference reference) :
        base(source, parentReference, reference)
    {
        FunctionName = functionName;
        Expression = expression;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens))
        {
            return ParseExpressionResult.Invalid<BracketedExpression>("Not valid.");
        }

        var matchingClosingParenthesis = FindMatchingClosingBracket(tokens);
        if (matchingClosingParenthesis == -1)
        {
            return ParseExpressionResult.Invalid<BracketedExpression>("No closing bracket found.");
        }

        var expressionReference = new NodeReference();
        var functionName = tokens.TokenValue(0);
        var innerExpressionTokens = tokens.TokensRange(2, matchingClosingParenthesis - 1);
        var innerExpression = factory.Parse(expressionReference, innerExpressionTokens, source.Line);
        if (!innerExpression.IsSuccess) return innerExpression;

        var reference = source.CreateReference();

        var expression = new BracketedExpression(functionName, innerExpression.Result, parentReference, source, reference);
        expressionReference.SetNode(expression);
        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length > 1
               && tokens.IsTokenType<StringLiteralToken>(0)
               && tokens.IsOperatorToken(1, OperatorType.OpenBrackets);
    }

    private static int FindMatchingClosingBracket(TokenList tokens)
    {
        Assert.NotNull(tokens, nameof(tokens));

        var count = 0;
        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];
            if (!(token is OperatorToken operatorToken)) continue;

            if (operatorToken.Type == OperatorType.OpenBrackets)
            {
                count++;
            }
            else if (operatorToken.Type == OperatorType.CloseBrackets)
            {
                count--;
                if (count == 0) return index;
            }
        }

        return -1;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Expression;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Type DeriveType(IValidationContext context)
    {
        return Expression.DeriveType(context);
    }

    public override Symbol GetSymbol() => null;
}
