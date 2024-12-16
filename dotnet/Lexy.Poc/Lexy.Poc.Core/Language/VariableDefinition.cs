using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class VariableDefinition
    {
        public string Default { get; }
        public string Type { get; }
        public string Name { get; }

        public VariableDefinition(string name, string type, string @default = null)
        {
            Type = type;
            Name = name;
            Default = @default;
        }

        public static VariableDefinition Parse(Line line)
        {
            var parts = line.Content.Trim().Split(" ");
            if (parts.Length == 2) return new VariableDefinition(parts[1], parts[0]);
            if (parts.Length == 4)
            {
                if (parts[2] != "=")
                    throw new InvalidOperationException("'=' expected. Invalid variable definition on line: " + line);
                return new VariableDefinition(parts[1], parts[0], parts[3]);
            }

            throw new InvalidOperationException("Invalid variable definition on line: " + line);
        }
    }
}