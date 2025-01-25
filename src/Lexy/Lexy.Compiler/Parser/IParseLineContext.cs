using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Parser;

public interface IParseLineContext
{
    Line Line { get; }
    IParserLogger Logger { get; }

    IExpressionFactory ExpressionFactory { get; }

    TokenValidator ValidateTokens<T>();
    TokenValidator ValidateTokens(string name);
}