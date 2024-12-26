using Lexy.Poc.Core.Language.Expressions.Functions;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class ParseBuiltInFunctionsResult
    {
        public string ErrorMessage { get; }
        public bool IsSuccess { get; }
        public BuiltInFunction Result { get; }

        private ParseBuiltInFunctionsResult(BuiltInFunction result)
        {
            Result = result;
            IsSuccess = true;
        }

        private ParseBuiltInFunctionsResult(bool success, string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccess = success;
        }

        public static ParseBuiltInFunctionsResult Success(BuiltInFunction result = null)
        {
            return new ParseBuiltInFunctionsResult(result);
        }

        public static ParseBuiltInFunctionsResult Failed(string errorMessage)
        {
            return new ParseBuiltInFunctionsResult(false, errorMessage);
        }
    }
}