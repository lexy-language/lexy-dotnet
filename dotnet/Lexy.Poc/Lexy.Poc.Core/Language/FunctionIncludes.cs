using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionIncludes : IToken
    {
        public IList<FunctionInclude> Definitions { get; } = new List<FunctionInclude>();

        public IToken Parse(Line line)
        {
            if (line.IsEmpty()) return this;

            var definition = FunctionInclude.Parse(line);
            Definitions.Add(definition);
            return this;
        }

        public bool Contains(string type)
        {
            return Definitions.Any(definition => definition.Name == type);
        }
    }
}