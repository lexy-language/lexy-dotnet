namespace Lexy.Compiler.Parser;

public interface IParseLineContext
{
    Line Line { get; }
    IParserLogger Logger { get; }

    TokenValidator ValidateTokens<T>();
    TokenValidator ValidateTokens(string name);
}