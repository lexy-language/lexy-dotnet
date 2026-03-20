using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser.Context;

public interface IParseLineContext
{
    Line Line { get; }
    IParserLogger Logger { get; }

    IDocumentSymbols Symbols { get; }

    TokenValidator ValidateTokens<T>();
}
