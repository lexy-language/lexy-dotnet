using System;
using Lexy.Compiler.Language.Expressions;

namespace Lexy.Compiler.Parser;

public class TokenizeResult : ParseResult<TokenList>
{
    public SourceReference Reference { get; }

    private TokenizeResult(TokenList result) : base(result)
    {
    }

    private TokenizeResult(bool success, SourceReference sourceReference, string errorMessage) : base(success, errorMessage)
    {
        Reference = sourceReference;
    }

    public static TokenizeResult Success(TokenList result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        return new TokenizeResult(result);
    }

    public static TokenizeResult Failed(SourceReference reference, string errorMessage)
    {
        return new TokenizeResult(false, reference, errorMessage);
    }
}