using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser;

public class ParseLineContext : IParseLineContext
{
    public Line Line { get; }
    public IParserLogger Logger { get; }
    public IExpressionFactory ExpressionFactory { get; }

    public ParseLineContext(Line line, IParserLogger logger, IExpressionFactory expressionFactory)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ExpressionFactory = expressionFactory ?? throw new ArgumentNullException(nameof(expressionFactory));
    }

    public TokenValidator ValidateTokens<T>()
    {
        return new TokenValidator(typeof(T).Name, Line, Logger);
    }
}