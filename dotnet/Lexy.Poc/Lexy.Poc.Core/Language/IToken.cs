using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public interface IToken
    {
        IToken Parse(Line line);
    }
}