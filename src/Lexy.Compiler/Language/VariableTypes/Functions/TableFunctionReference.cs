using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Functions;

internal abstract class TableFunctionReference : IInstanceFunction
{
    protected Table Table { get; }

    protected abstract string FunctionHelp { get; }

    protected TableFunctionReference(Table table)
    {
        Table = table ?? throw new ArgumentNullException(nameof(table));
    }

    public abstract ValidateInstanceFunctionArgumentsResult ValidateArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments,
        SourceReference reference);

    public abstract VariableType GetResultsType(IReadOnlyList<Expression> arguments);

    protected  bool ValidateTable(IValidationContext context, SourceReference reference)
    {
        if (Table.Header == null || Table.Header.Columns.Count < 2)
        {
            context.Logger.Fail(reference, $"At least 2 columns expected for table '{Table.Name}'. {FunctionHelp}");
            return false;
        }
        return true;
    }

    protected void ValidateColumnValueType(IValidationContext context, IReadOnlyList<Expression> arguments,
        int valueColumn, string argumentName, ColumnHeader columnHeader, SourceReference reference)
    {
        var valueType = arguments[valueColumn].DeriveType(context);
        ValidateColumnValueType(context, valueColumn, argumentName, columnHeader.Name, valueType, columnHeader.Type.VariableType, reference);
    }

    private void ValidateColumnValueType(IValidationContext context, int argumentIndex, string argumentName, string columnName, VariableType valueType,
        VariableType columnType, SourceReference reference)
    {
        if (valueType == null)
        {
            context.Logger.Fail(reference,
                $"Invalid argument {argumentIndex + 1}. Should be {argumentName} column. {FunctionHelp}");
        }
        else if (!valueType.Equals(columnType))
        {
            context.Logger.Fail(reference,
                $"Invalid column type '{columnName}': '{columnType}' doesn't match condition type '{valueType}'. {FunctionHelp}");
        }
    }

    protected ColumnHeader GetColumn(IValidationContext context, IReadOnlyList<Expression> arguments, int? argumentIndex, int? defaultColumn, SourceReference reference)
    {
        if (argumentIndex == null)
        {
            if (defaultColumn == null)
            {
                throw new InvalidOperationException("Default column should not be null");
            }
            return Table.Header.GetColumn(defaultColumn.Value);
        }

        var index = argumentIndex.Value;
        if (arguments[argumentIndex.Value] is not MemberAccessExpression column)
        {
            context.Logger.Fail(reference, $"Invalid column at argument '{index + 1}'. {FunctionHelp}");
            return null;
        }

        return GetColumnHeader(context, index, column, reference);
    }

    private ColumnHeader GetColumnHeader(IValidationContext context, int argumentIndex, MemberAccessExpression column, SourceReference reference)
    {
        if (!ValidateColumn(context, column.VariablePath, argumentIndex, reference)) return null;

        var columnHeader = Table.Header?.Get(column.VariablePath);
        if (columnHeader == null)
        {
            context.Logger.Fail(reference,
                $"Invalid argument {argumentIndex}. Column name '{column}' not found in table '{Table.Name}'. ${FunctionHelp}");
            return null;
        }

        return columnHeader;
    }

    private bool ValidateColumn(IValidationContext context, IdentifierPath columnIdentifier, int index, SourceReference reference)
    {
        if (columnIdentifier.RootIdentifier != Table.Name.Value || columnIdentifier.Parts != 2)
        {
            context.Logger.Fail(reference,
                $"Invalid argument {index}. Result column table '{columnIdentifier.RootIdentifier}' should be table name '{Table.Name}'. {FunctionHelp}");
            return false;
        }

        return true;
    }
}