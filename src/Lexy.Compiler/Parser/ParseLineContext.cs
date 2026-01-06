using System;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public class ParseLineContext : IParseLineContext
{
    public Line Line { get; }
    public IParserLogger Logger { get; }
    public IExpressionFactory ExpressionFactory { get; }

    public ParseLineContext(Line line, IParserLogger logger, IExpressionFactory expressionFactory)
    {
        Line = Assert.NotNull(line, nameof(line));
        Logger = Assert.NotNull(logger, nameof(logger));
        ExpressionFactory = Assert.NotNull(expressionFactory, nameof(expressionFactory));
    }

    public TokenValidator ValidateTokens<T>()
    {
        return new TokenValidator(typeof(T).Name, Line, Logger);
    }
}