using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class TableHeader
    {
        public string Name { get; }
        public string Type { get; }

        public TableHeader(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public static TableHeader Parse(string value, Line line)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var parts = value.Trim().Split(' ');
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"Invalid table header: {value}{Environment.NewLine}{line}");
            }

            return new TableHeader(parts[1], parts[0]);
        }
    }
}