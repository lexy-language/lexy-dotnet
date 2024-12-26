using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class ParserResult
    {
        public Nodes Nodes { get; }

        public ParserResult(Nodes nodes)
        {
            Nodes = nodes;
        }
    }
}