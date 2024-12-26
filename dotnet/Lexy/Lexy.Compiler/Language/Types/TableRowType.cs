using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public class TableRowType : ComplexTypeType
    {
        public string TableName { get; }

        public TableRowType(string tableName) : base(tableName)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public override ComplexType GetComplexType(IValidationContext context) =>
            context.Nodes.GetTable(TableName)?.GetRowType(context);
    }
}