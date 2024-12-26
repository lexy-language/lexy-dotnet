using Lexy.Compiler.Language.Expressions.Functions;

namespace Lexy.Compiler.Language.Expressions
{
    public class ParseExpressionFunctionsResult
    {
        public string ErrorMessage { get; }
        public bool IsSuccess { get; }

        public ExpressionFunction Result { get; }

        private ParseExpressionFunctionsResult(ExpressionFunction result)
        {
            Result = result;
            IsSuccess = true;
        }

        private ParseExpressionFunctionsResult(bool success, string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccess = success;
        }

        public static ParseExpressionFunctionsResult Success(ExpressionFunction result = null)
        {
            return new ParseExpressionFunctionsResult(result);
        }

        public static ParseExpressionFunctionsResult Failed(string errorMessage)
        {
            return new ParseExpressionFunctionsResult(false, errorMessage);
        }
    }
}