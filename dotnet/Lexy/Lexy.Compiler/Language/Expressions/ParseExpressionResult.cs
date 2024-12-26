namespace Lexy.Poc.Core.Language.Expressions
{
    public class ParseExpressionResult
    {
        public Expression Expression { get; }
        public string ErrorMessage { get; }
        public ParseExpressionStatus Status { get; }

        private ParseExpressionResult(ParseExpressionStatus status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Status = status;
        }

        private ParseExpressionResult(ParseExpressionStatus status, Expression expression = null)
        {
            Expression = expression;
            Status = status;
        }

        public static ParseExpressionResult Invalid<T>(string error)
        {
            return new ParseExpressionResult(ParseExpressionStatus.Failed, $"({typeof(T).Name}) {error}");
        }

        public static ParseExpressionResult Success(Expression expression)
        {
            return new ParseExpressionResult(ParseExpressionStatus.Success, expression);
        }
    }
}