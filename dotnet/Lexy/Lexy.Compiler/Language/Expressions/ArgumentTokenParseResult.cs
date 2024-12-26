using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class ArgumentTokenParseResult
    {
        public string ErrorMessage { get; }
        public bool IsSuccess { get; }
        public IEnumerable<TokenList> Result { get; }

        private ArgumentTokenParseResult(IEnumerable<TokenList> result)
        {
            Result = result;
            IsSuccess = true;
        }

        private ArgumentTokenParseResult(bool success, string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccess = success;
        }

        public static ArgumentTokenParseResult Success(IEnumerable<TokenList> result = null)
        {
            return new ArgumentTokenParseResult(result ?? new TokenList[]{});
        }

        public static ArgumentTokenParseResult Failed(string errorMessage)
        {
            return new ArgumentTokenParseResult(false, errorMessage);
        }
    }
}