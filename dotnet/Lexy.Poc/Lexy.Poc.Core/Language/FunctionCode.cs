using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionCode : IToken
    {
        public IList<string> Lines { get; } = new List<string>();

        public IToken Parse(Line line)
        {
            Lines.Add(line.TrimmedContent);
            return this;
        }
    }
}