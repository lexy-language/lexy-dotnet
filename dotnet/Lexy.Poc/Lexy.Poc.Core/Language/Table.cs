using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Table : RootComponent
    {
        public TableName Name { get; } = new TableName();
        public Comments Comments { get; } = new Comments();
        public TableHeaders Headers { get; private set; }
        public IList<TableRow> Rows { get; } = new List<TableRow>();
        public override string ComponentName => Name.Value;

        private Table(string name)
        {
            Name.ParseName(name);
        }

        internal static Table Parse(ComponentName name)
        {
            return new Table(name.Name);
        }

        public override IComponent Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty())
            {
                return this;
            }

            if (line.IsComment())
            {
                Comments.Parse(context);
            }
            else if (Headers == null)
            {
                Headers = TableHeaders.Parse(context);
            }
            else
            {
                Rows.Add(TableRow.Parse(context));
            }

            return this;
        }
    }
}