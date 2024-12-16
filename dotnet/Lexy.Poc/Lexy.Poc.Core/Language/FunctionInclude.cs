using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionInclude
    {
        public string Type { get; }
        public string Name { get; }

        public FunctionInclude(string name, string type)
        {
            Type = type;
            Name = name;
        }

        public static FunctionInclude Parse(Line line)
        {
            var parts = line.Content.Trim().Split(" ");
            if (parts.Length == 2)
            {
                return new FunctionInclude(parts[1], parts[0]);
            }

            throw new InvalidOperationException("Function Include definition on line: " + line);
        }
    }
}