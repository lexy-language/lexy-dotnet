namespace Lexy.Compiler.Parser;

public interface ITokenizer
{
    TokenList Tokenize(Line line, IParserContext parserContext, out bool errors);
}