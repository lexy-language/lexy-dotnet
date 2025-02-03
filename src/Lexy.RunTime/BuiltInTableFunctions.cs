using System;
using System.Collections.Generic;

namespace Lexy.RunTime;

public static class BuiltInTableFunctions
{
    public static TResult LookUp<TCondition, TRow, TResult>(
        string resultName,
        string valueName,
        string tableName,
        IReadOnlyList<TRow> tableValues,
        TCondition condition,
        Func<TRow, TCondition> getValue,
        Func<TRow, TResult> getResult,
        IExecutionContext context)
        where TRow : class
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        if (getResult == null) throw new ArgumentNullException(nameof(getResult));
        var functionName = $"Lookup '{resultName}' by '{valueName}' from table '{tableName}'";

        TRow lastRow = null;

        for (var index = 0; index < tableValues.Count; index++)
        {
            var row = tableValues[index];
            var value = getValue(row);

            var valueComparedToCondition = value.CompareTo(condition);
            if (valueComparedToCondition == 0)
            {
                context.LogChild($"{functionName} returned value from row: {index + 1}");
                return getResult(row);
            }

            if (valueComparedToCondition > 0)
            {
                context.LogChild($"{functionName} returned value from previous row: {index}");

                if (lastRow == null)
                    throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");

                return getResult(lastRow);
            }

            lastRow = row;
        }

        if (lastRow == null)
            throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");

        context.LogChild($"{functionName} returned value from last row: {tableValues.Count}");
        return getResult(lastRow);
    }

    public static TRow LookUpRow<TCondition, TRow>(
        string valueName,
        string tableName,
        IReadOnlyList<TRow> tableValues,
        TCondition condition,
        Func<TRow, TCondition> getValue,
        IExecutionContext context)
        where TRow : class
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        var functionName = $"LookupRow' by '{valueName}' from table '{tableName}'";

        TRow lastRow = null;

        for (var index = 0; index < tableValues.Count; index++)
        {
            var row = tableValues[index];
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
                    throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");

                return lastRow;
            }

            lastRow = row;
        }

        if (lastRow == null)
            throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");

        context.LogChild($"{functionName} returned value from last row: {tableValues.Count}");
        return lastRow;
    }

    public static TResult LookUpBy<TDiscriminator, TCondition, TRow, TResult>(
        string resultName,
        string discriminatorName,
        string valueName,
        string tableName,
        IReadOnlyList<TRow> tableValues,
        TDiscriminator discriminator,
        TCondition condition,
        Func<TRow, TDiscriminator> getDiscriminator,
        Func<TRow, TCondition> getValue,
        Func<TRow, TResult> getResult,
        IExecutionContext context)
        where TRow : class
        where TDiscriminator : IComparable
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));
        if (getResult == null) throw new ArgumentNullException(nameof(getResult));

        var functionName = $"Lookup '{resultName}' by discriminator '{discriminatorName}' and value '{valueName}' from table '{tableName}'";

        TRow lastRow = null;

        for (var index = 0; index < tableValues.Count; index++)
        {
            var row = tableValues[index];
            var discriminatorValue = getDiscriminator(row);
            if (discriminator.CompareTo(discriminatorValue) != 0) continue;

            var value = getValue(row);
            var valueComparedToCondition = value.CompareTo(condition);
            if (valueComparedToCondition == 0)
            {
                context.LogChild($"{functionName} returned value from row: {index + 1}");
                return getResult(row);
            }

            if (valueComparedToCondition > 0)
            {
                context.LogChild($"{functionName} returned value from previous row: {index}");

                if (lastRow == null)
                {
                    throw new ExecutionException(
                        $"{functionName} failed. Discriminator '{discriminator}' and search value '{condition}' not found.");
                }
                return getResult(lastRow);
            }

            lastRow = row;
        }

        if (lastRow == null)
        {
            throw new ExecutionException(
                $"{functionName} failed. Discriminator '{discriminator}' and search value '{condition}' not found.");
        }

        context.LogChild($"{functionName} returned value from last row: {tableValues.Count}");
        return getResult(lastRow);
    }

    public static TRow LookUpRowBy<TDiscriminator, TCondition, TRow>(
        string discriminatorName,
        string valueName,
        string tableName,
        IReadOnlyList<TRow> tableValues,
        TDiscriminator discriminator,
        TCondition condition,
        Func<TRow, TDiscriminator> getDiscriminator,
        Func<TRow, TCondition> getValue,
        IExecutionContext context)
        where TRow : class
        where TDiscriminator : IComparable
        where TCondition : IComparable
    {
        if (tableValues == null) throw new ArgumentNullException(nameof(tableValues));
        if (getValue == null) throw new ArgumentNullException(nameof(getValue));

        var functionName = $"LookupRow' by by discriminator '{discriminatorName}' and value '{valueName}' from table '{tableName}'";

        TRow lastRow = null;

        for (var index = 0; index < tableValues.Count; index++)
        {
            var row = tableValues[index];
            var discriminatorValue = getDiscriminator(row);
            if (discriminator.CompareTo(discriminatorValue) != 0) continue;

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
                        $"{functionName} failed. Discriminator '{discriminator}' and search value '{condition}' not found.");
                }
                return lastRow;
            }

            lastRow = row;
        }

        if (lastRow == null)
        {
            throw new ExecutionException($"{functionName} failed. Discriminator '{discriminator}' and search value '{condition}' not found.");
        }

        context.LogChild($"{functionName} returned value from last row: {tableValues.Count}");
        return lastRow;
    }
}