using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public interface IExpressionFactory
{
    ParseExpressionResult Parse(INode parent, TokenList tokens, Line currentLine);
    ParseExpressionResult Parse(NodeReference parentReference, TokenList tokens, Line currentLine);
}
