namespace Lexy.Compiler.Language.Expressions;

public sealed class ParseVariableNameExpressionResult : ParseResult<VariableNameExpression>
{
    private ParseVariableNameExpressionResult(VariableNameExpression result) : base(result)
    {
    }

    private ParseVariableNameExpressionResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static ParseVariableNameExpressionResult Invalid<T>(string error)
    {
        return new ParseVariableNameExpressionResult(false, $"({typeof(T).Name}) {error}");
    }

    public static ParseVariableNameExpressionResult Success(VariableNameExpression expression)
    {
        return new ParseVariableNameExpressionResult(expression);
    }
}