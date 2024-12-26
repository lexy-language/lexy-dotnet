using System;
using System.Collections.Generic;

namespace Lexy.Poc.Core.RunTime
{
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
            where TCondition : IComparable<TCondition>
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
                    context.LogDebug($"{functionName} returned value from row: {index + 1}");
                    return getResult(row);
                }

                if (valueComparedToCondition > 0)
                {
                    context.LogDebug($"{functionName} returned value from previous row: {index}");

                    if (lastRow == null)
                    {
                        throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");
                    }

                    return getResult(lastRow);
                }

                lastRow = row;
            }

            if (lastRow == null)
            {
                throw new ExecutionException($"{functionName} failed. Search value '{condition}' not found.");
            }

            context.LogDebug($"{functionName} returned value from last row: {tableValues.Count}");
            return getResult(lastRow);
        }
    }
}