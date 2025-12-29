using System;
using System.Collections.Generic;

namespace Lexy.RunTime.Libraries;

public static class Table
{
    public static TResult LookUp<TCondition, TRow, TResult>(
        string valueName,
        TCondition condition,
        Func<TRow, TCondition> getValue,

        string tableName,
        IReadOnlyList<TRow> tableValues,

        string resultName,
        Func<TRow, TResult> getResult,

        IExecutionContext context)
        where TRow : class
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        if (getResult == null) throw new ArgumentNullException(nameof(getResult));

        var functionName = $"Lookup '{resultName}' by '{valueName}' from table '{tableName}'";
        var row = LookUp(tableValues, condition, _ => true, getValue, context, functionName);
        return getResult(row);
    }

    public static TRow LookUpRow<TCondition, TRow>(
        string valueName,
        TCondition condition,
        Func<TRow, TCondition> getValue,

        string tableName,
        IReadOnlyList<TRow> tableValues,

        IExecutionContext context)
        where TRow : class
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));

        var functionName = $"LookupRow' by '{valueName}' from table '{tableName}'";
        return LookUp(tableValues, condition, _ => true, getValue, context, functionName);
    }

    public static TResult LookUp<TDiscriminator, TCondition, TRow, TResult>(
        string discriminatorName,
        TDiscriminator discriminator,
        Func<TRow, TDiscriminator> getDiscriminator,

        string valueName,
        TCondition condition,
        Func<TRow, TCondition> getValue,

        string tableName,
        IReadOnlyList<TRow> tableValues,

        string resultName,
        Func<TRow, TResult> getResult,

        IExecutionContext context)
        where TRow : class
        where TDiscriminator : IComparable
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        if (getResult == null) throw new ArgumentNullException(nameof(getResult));

        bool CheckDiscriminator(TRow row)
        {
            var discriminatorValue = getDiscriminator(row);
            return discriminator.CompareTo(discriminatorValue) == 0;
        }

        var functionName = $"Lookup '{resultName}' by discriminator '{discriminatorName}' and value '{valueName}' from table '{tableName}'";
        var row = LookUp(tableValues, condition, CheckDiscriminator, getValue, context, functionName);
        return getResult(row);
    }

    public static TRow LookUpRow<TDiscriminator, TCondition, TRow>(
        string discriminatorName,
        TDiscriminator discriminator,
        Func<TRow, TDiscriminator> getDiscriminator,

        string valueName,
        TCondition condition,
        Func<TRow, TCondition> getValue,

        string tableName,
        IReadOnlyList<TRow> tableValues,

        IExecutionContext context)
        where TRow : class
        where TDiscriminator : IComparable
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));

        bool CheckDiscriminator (TRow row)
        {
            var discriminatorValue = getDiscriminator(row);
            return discriminator.CompareTo(discriminatorValue) == 0;
        }

        var functionName = $"LookupRow' by by discriminator '{discriminatorName}' and value '{valueName}' from table '{tableName}'";
        return LookUp(tableValues, condition, CheckDiscriminator, getValue, context, functionName);
    }

    private static TRow LookUp<TCondition, TRow>(
        IReadOnlyList<TRow> tableValues,
        TCondition condition,
        Func<TRow, bool> checkDiscriminator,
        Func<TRow, TCondition> getValue,
        IExecutionContext context,
        string functionName)
        where TRow : class
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));

        TRow lastRow = null;

        for (var index = 0; index < tableValues.Count; index++)
        {
            var row = tableValues[index];
            if (!checkDiscriminator(row)) continue;

            var value = getValue(row);
            var valueComparedToCondition = value.CompareTo(condition);
            if (valueComparedToCondition == 0)
            {
                context.LogChild($"{functionName} returned value from row: {index + 1}");
                return row;
            }

            if (valueComparedToCondition > 0)
            {
                context.LogChild($"{functionName} returned value from previous row: {index}");

                if (lastRow == null)
                {
                    throw new ExecutionException(
                        $"{functionName} failed. Search value '{condition}' not found.");
                }
                return lastRow;
            }

            lastRow = row;
        }

        if (lastRow == null)
        {
            throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");
        }

        context.LogChild($"{functionName} returned value from last row: {tableValues.Count}");
        return lastRow;
    }
}