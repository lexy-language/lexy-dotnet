using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public class TokenizeResult : ParseResult<TokenList>
{
    public SourceReference Reference { get; }

    private TokenizeResult(TokenList result) : base(result)
    {
    }

    private TokenizeResult(bool success, SourceReference sourceReference, string errorMessage) :
        base(success, errorMessage)
    {
        Reference = sourceReference;
    }

    public static TokenizeResult Success(TokenList result)
    {
        Assert.NotNull(result, nameof(result));

        return new TokenizeResult(result);
    }

    public static TokenizeResult Failed(SourceReference reference, string errorMessage)
    {
        return new TokenizeResult(false, reference, errorMessage);
    }
}