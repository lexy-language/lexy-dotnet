using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

internal abstract class TableFunction : FunctionCallExpression, IHasNodeDependencies
{
    public abstract string FunctionHelp { get; }
    public string TableName { get; }
    public Table Table { get; private set; }

    protected TableFunction(string tableName, string functionName, ExpressionSource source)
        : base(functionName, source)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public IEnumerable<IRootNode> GetDependencies(IRootNodeList rootNodeList)
    {
        var table = rootNodeList.GetTable(TableName);
        if (table != null) yield return table;
    }

    protected override void Validate(IValidationContext context)
    {
        Table = context.RootNodes.GetTable(TableName);
        if (Table == null)
        {
            context.Logger.Fail(Reference,
                $"Invalid argument 0. Table name '{TableName}' not found. {FunctionHelp}");
        }
    }


    protected ColumnHeader GetColumnHeader(IValidationContext context, int argumentIndex, MemberAccessLiteral column)
    {
        if (!ValidateColumn(context, column, argumentIndex)) return null;

        var columnHeader = Table?.Header?.Get(column);
        if (columnHeader == null)
        {
            context.Logger.Fail(Reference,
                $"Invalid argument {argumentIndex}. Column name '{column}' not found in table '{TableName}'. ${FunctionHelp}");
            return null;
        }

        return columnHeader;
    }

    private bool ValidateColumn(IValidationContext context, MemberAccessLiteral column, int index)
    {
        if (column.Parent != TableName || column.Parts.Length != 2)
        {
            context.Logger.Fail(Reference,
                $"Invalid argument {index}. Result column table '{column.Parent}' should be table name '{TableName}'. {FunctionHelp}");
            return false;
        }

        return true;
    }

    protected void ValidateColumnValueType(int argumentIndex, MemberAccessLiteral columnName, VariableType valueType,
        VariableType columnType, IValidationContext context)
    {
        if (valueType == null || !valueType.Equals(columnType))
        {
            context.Logger.Fail(Reference,
                $"Invalid argument ${argumentIndex}. Column type '${columnName}': '${columnType}' doesn't match condition type '${valueType}'. ${FunctionHelp}");
        }
    }
}