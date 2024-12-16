using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioFunctionName : IToken
    {
        public string Value { get; private set; }

        public IToken Parse(Line line)
        {
            Value = line.Parameter();
            return this;
        }

        public override string ToString() => Value;
    }
}