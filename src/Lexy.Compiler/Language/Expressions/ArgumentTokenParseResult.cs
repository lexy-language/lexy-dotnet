using System.Collections.Generic;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public sealed class ArgumentTokenParseResult : ParseResult<IEnumerable<TokenList>>
{
    private ArgumentTokenParseResult(IEnumerable<TokenList> result) : base(result)
    {
    }

    private ArgumentTokenParseResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static ArgumentTokenParseResult Success(IEnumerable<TokenList> result)
    {
        return new ArgumentTokenParseResult(result ?? new TokenList[] { });
    }

    public static ArgumentTokenParseResult Failed(string errorMessage)
    {
        return new ArgumentTokenParseResult(false, errorMessage);
    }
}