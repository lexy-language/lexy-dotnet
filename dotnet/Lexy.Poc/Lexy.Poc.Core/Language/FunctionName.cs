using System;
using System.Linq;
using System.Text;

namespace Lexy.Poc.Core.Language
{
    public class FunctionName
    {
        public string Value { get; private set; } = Guid.NewGuid().ToString("D");

        public void ParseName(string parameter)
        {
            Value = parameter;
        }

        public string ClassName()
        {
            var nameBuilder = new StringBuilder("Function");
            foreach (var @char in Value.Where(char.IsLetter))
            {
                nameBuilder.Append(@char);
            }
            return nameBuilder.ToString();
        }
    }
}