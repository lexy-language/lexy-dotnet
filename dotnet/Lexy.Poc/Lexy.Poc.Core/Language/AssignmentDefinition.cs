using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class AssignmentDefinition
    {
        public string Value { get; }
        public string Name { get; }

        public AssignmentDefinition(string name, string value)
        {
            Value = value;
            Name = name;
        }

        public static AssignmentDefinition Parse(Line line)
        {
            var parts = line.Content.Trim().Split("=");
            if (parts.Length == 2)
            {
                return new AssignmentDefinition(parts[0].Trim(), parts[1].Trim());
            }

            throw new InvalidOperationException("Invalid assignment definition on line: " + line);
        }
    }
}