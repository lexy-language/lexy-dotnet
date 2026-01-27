namespace Lexy.Compiler.Parser.Tokens;

public interface ITokenizer
{
    TokenizeResult Tokenize(Line line);
}
