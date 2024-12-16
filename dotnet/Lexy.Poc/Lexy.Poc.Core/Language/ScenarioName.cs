using System;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioName
    {
        public string Value { get; private set; } = Guid.NewGuid().ToString("D");

        public void ParseName(string parameter)
        {
            Value = parameter;
        }

        public override string ToString() => Value;
    }
}