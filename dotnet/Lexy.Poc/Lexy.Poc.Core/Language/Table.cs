using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Table : RootToken
    {
        public TableName Name { get; } = new TableName();
        public Comments Comments { get; } = new Comments();
        public TableHeaders Headers { get; private set; }
        public IList<TableRow> Rows { get; } = new List<TableRow>();
        public override string TokenName => Name.Value;

        private Table(string name)
        {
            Name.ParseName(name);
        }

        internal static Table Parse(TokenName name)
        {
            return new Table(name.Parameter);
        }

        public override IToken Parse(Line line)
        {
            if (line.IsComment())
            {
                Comments.Parse(line);
            }
            else if (Headers == null)
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