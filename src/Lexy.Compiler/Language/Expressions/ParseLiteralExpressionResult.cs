namespace Lexy.Compiler.Language.Expressions;

public sealed class ParseLiteralExpressionResult : ParseResult<LiteralExpression>
{
    private ParseLiteralExpressionResult(LiteralExpression result) : base(result)
    {
    }

    private ParseLiteralExpressionResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static ParseLiteralExpressionResult Invalid<T>(string error)
    {
        return new ParseLiteralExpressionResult(false, $"({typeof(T).Name}) {error}");
    }

    public static ParseLiteralExpressionResult Success(LiteralExpression expression)
    {
        return new ParseLiteralExpressionResult(expression);
    }
}
