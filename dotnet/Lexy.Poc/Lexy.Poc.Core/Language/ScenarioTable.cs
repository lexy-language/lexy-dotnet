using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class ScenarioTable : IToken
    {
        public TableHeaders Headers { get; private set; }
        public IList<TableRow> Rows { get; } = new List<TableRow>();

        public IToken Parse(Line line)
        {
            if (line.IsEmpty()) return this;

            if (Headers == null)
            {
                Headers = TableHeaders.Parse(line);
            }
            else
            {
                Rows.Add(TableRow.Parse(line));
            }

            return this;
        }
    }
}