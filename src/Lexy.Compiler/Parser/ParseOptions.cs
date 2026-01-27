namespace Lexy.Compiler.Parser;

public class ParseOptions
{
    public bool SuppressException { get; init; }

    public static ParseOptions Default()
    {
        return new ParseOptions { SuppressException = false };
    }
}
