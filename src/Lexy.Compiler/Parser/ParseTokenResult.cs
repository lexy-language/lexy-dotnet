using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser;

public class ParseTokenResult
{
    public string ValidationError { get; }
    public Token NewToken { get; }
    public TokenStatus Status { get; }
    public bool CharProcessed { get; }

    private ParseTokenResult(TokenStatus status, bool charProcessed, Token newToken = null, string validationError = null)
    {
        NewToken = newToken;
        CharProcessed = charProcessed;
        Status = status;
        ValidationError = validationError;
    }

    private ParseTokenResult(TokenStatus status, string validationError)
    {
        ValidationError = validationError;
        Status = status;
    }

    public static ParseTokenResult InProgress(ParsableToken newToken = null)
    {
        return new ParseTokenResult(TokenStatus.InProgress, true, newToken);
    }

    public static ParseTokenResult Finished(bool charProcessed, Token newToken = null, string error = null)
    {
        if (error != null)
        {
            return new ParseTokenResult(TokenStatus.InvalidToken, charProcessed, newToken, error);
        }
        return new ParseTokenResult(TokenStatus.Finished, charProcessed, newToken);
    }

    public static ParseTokenResult Invalid(string validationError)
    {
        return new ParseTokenResult(TokenStatus.InvalidToken, validationError);
    }
}
