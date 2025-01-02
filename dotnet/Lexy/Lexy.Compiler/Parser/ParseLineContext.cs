using System;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser;

public class ParseLineContext : IParseLineContext
{
    public Line Line { get; }
    public IParserLogger Logger { get; }

    public ParseLineContext(Line line, IParserLogger logger)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TokenValidator ValidateTokens<T>()
    {
        return new TokenValidator(typeof(T).Name, Line, Logger);
    }

    public TokenValidator ValidateTokens(string name)
    {
        return new TokenValidator(name, Line, Logger);
    }
}