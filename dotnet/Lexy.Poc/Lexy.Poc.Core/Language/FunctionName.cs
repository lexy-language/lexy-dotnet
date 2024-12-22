using System;
using System.Linq;
using System.Text;

namespace Lexy.Poc.Core.Language
{
    public class FunctionName
    {
        public string Value { get; private set; }

        public void ParseName(string parameter = null)
        {
            Value = parameter ?? Guid.NewGuid().ToString("D");
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