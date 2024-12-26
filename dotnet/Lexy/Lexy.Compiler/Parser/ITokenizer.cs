using System.Linq;

namespace Lexy.Poc.Core.Parser
{
    public interface ITokenizer
    {
        TokenList Tokenize(Line line, IParserContext parserContext, out bool errors);
    }
}