using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal abstract class TableFunction : ObjectFunction
{
    protected Table Table { get; }

    protected abstract string FunctionHelp { get; }

    protected TableFunction(string name, Table table) : base(name, table.CreateType())
    {
        Table = Assert.NotNull(table, nameof(table));
    }

    protected bool ValidateTable(IValidationContext context, SourceReference reference)
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
        ValidateColumnValueType(context, valueColumn, argumentName, columnHeader.Name, valueType, columnHeader.TypeDeclaration.Type, reference);
    }

    private void ValidateColumnValueType(IValidationContext context, int argumentIndex, string argumentName, string columnName, Type valueType,
        Type columnType, SourceReference reference)
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

    protected ColumnHeader ValidatorDiscriminator(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference,
        IOverloadArguments overloadArguments)
    {
        if (overloadArguments.Discriminator == null) return null;

        var discriminatorColumnHeader = overloadArguments.DefaultDiscriminatorColumn != null
            ? GetColumn(context, arguments, overloadArguments.DiscriminatorColumnArgument, overloadArguments.DefaultDiscriminatorColumn, reference)
            : null;
        ValidateColumnValueType(context, arguments, overloadArguments.Discriminator.Value, "Discriminator", discriminatorColumnHeader, reference);
        return discriminatorColumnHeader;
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
        if (columnIdentifier.RootIdentifier != Table.Name || columnIdentifier.Parts != 2)
        {
            context.Logger.Fail(reference,
                $"Invalid argument {index}. Result column table '{columnIdentifier.RootIdentifier}' should be table name '{Table.Name}'. {FunctionHelp}");
            return false;
        }

        return true;
    }
}
