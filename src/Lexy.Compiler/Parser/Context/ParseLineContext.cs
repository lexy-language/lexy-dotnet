using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Context;

public class ParseLineContext : IParseLineContext
{
    public Line Line { get; }
    public IParserLogger Logger { get; }
    public IExpressionFactory ExpressionFactory { get; }
    public IDocumentSymbols Symbols { get; }

    public ParseLineContext(Line line, IParserLogger logger, IDocumentSymbols symbols, IExpressionFactory expressionFactory)
    {
        Line = Assert.NotNull(line, nameof(line));
        Logger = Assert.NotNull(logger, nameof(logger));
        ExpressionFactory = Assert.NotNull(expressionFactory, nameof(expressionFactory));
        Symbols = Assert.NotNull(symbols, nameof(symbols));
    }

    public TokenValidator ValidateTokens<T>()
    {
        return new TokenValidator(typeof(T).Name, Line, Logger);
    }
}
