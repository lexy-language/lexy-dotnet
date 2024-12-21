using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class ParserResult
    {
        public Components Components { get; }

        public ParserResult(Components components)
        {
            Components = components;
        }
    }
}