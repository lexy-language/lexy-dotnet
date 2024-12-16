using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class TableRow
    {
        public IList<string> Values { get; } = new List<string>();

        private TableRow(string[] values)
        {
            Values = values;
        }

        public static TableRow Parse(Line line)
        {
            var values = line.TrimmedContent
                .Trim(TokenNames.TableSeparator)
                .Split(TokenNames.TableSeparator)
                .Select(part => part.Trim())
                .ToArray();

            return new TableRow(values);
        }
    }
}