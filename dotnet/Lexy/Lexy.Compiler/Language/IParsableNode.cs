using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public interface IParsableNode : INode
    {
        IParsableNode Parse(IParserContext context);
    }
}