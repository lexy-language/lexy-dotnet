using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioExpectError : IToken
    {
        public string Message { get; private set; }
        public bool HasValue { get => Message != null; }

        public IToken Parse(Line line)
        {
            Message = line.Parameter();
            return this;
        }
    }
}