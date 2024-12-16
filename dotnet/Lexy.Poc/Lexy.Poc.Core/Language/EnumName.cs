using System;

namespace Lexy.Poc.Core.Language
{
    public class EnumName
    {
        public string Value { get; private set; } = Guid.NewGuid().ToString("D");

        public void ParseName(string parameter)
        {
            Value = parameter;
        }
    }
}