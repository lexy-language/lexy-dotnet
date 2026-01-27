using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser;

public interface IParseLineContext
{
    Line Line { get; }
    IParserLogger Logger { get; }

    IExpressionFactory ExpressionFactory { get; }
    DocumentSymbols Symbols { get; }

    TokenValidator ValidateTokens<T>();
}
