using System;

namespace Lexy.Poc.Core.Parser
{
    internal class TokenName
    {
        public string Parameter { get; }
        public string Name { get; }

        private TokenName(string name, string parameter)
        {
            Parameter = parameter;
            Name = name;
        }

        public static TokenName Parse(Line line)
        {
            var content = line.Content;
            var indexOfSeparator = content.IndexOf(":", StringComparison.Ordinal);
            if (indexOfSeparator == -1) return null;

            var name = content[..indexOfSeparator].Trim();
            var parameter = content[(indexOfSeparator + 1)..].Trim();

            return new TokenName(name, parameter);
        }
    }
}