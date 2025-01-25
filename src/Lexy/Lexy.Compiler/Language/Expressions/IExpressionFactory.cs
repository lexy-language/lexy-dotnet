using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public interface IExpressionFactory
{
    ParseExpressionResult Parse(TokenList tokens, Line currentLine);
}