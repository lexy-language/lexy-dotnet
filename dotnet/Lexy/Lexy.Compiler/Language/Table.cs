using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language
{
    public class Table : RootNode
    {
        public TableName Name { get; } = new TableName();
        public TableHeader Header { get; private set; }
        public IList<TableRow> Rows { get; } = new List<TableRow>();

        public override string NodeName => Name.Value;

        public const string RowName = "Rw";

        private Table(string name, SourceReference reference) : base(reference)
        {
            Name.ParseName(name);
        }

        internal static Table Parse(NodeName name, SourceReference reference)
        {
            return new Table(name.Name, reference);
        }

        public override IParsableNode Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            if (line.IsEmpty()) return this;

            if (line.IsComment())
            {
                throw new InvalidOperationException("No comments expected. Comment should be parsed by Document only.");
            }

            if (IsFirstLine())
            {
                Header = TableHeader.Parse(context);
            }
            else
            {
                Rows.Add(TableRow.Parse(context));
            }

            return this;
        }

        private bool IsFirstLine()
        {
            return Header == null;
        }

        public override IEnumerable<INode> GetChildren()
        {
            if (Header != null)
            {
                yield return Header;
            }

            foreach (var row in Rows)
            {
                yield return row;
            }
        }

        protected override void Validate(IValidationContext context)
        {
        }

        public ComplexType GetRowType(IValidationContext context)
        {
            var members = Header.Columns
                .Select(column => new ComplexTypeMember(column.Name, column.Type.CreateVariableType(context)))
                .ToList();

            return new ComplexType(Name.Value, ComplexTypeSource.TableRow, members);
        }
    }
}