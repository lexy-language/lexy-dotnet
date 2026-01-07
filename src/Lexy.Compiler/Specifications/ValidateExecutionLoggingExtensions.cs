using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Language.Scenarios;
using Lexy.RunTime;

namespace Lexy.Compiler.Specifications;

public static class ValidateExecutionLoggingExtensions
{
    public static IEnumerable<string> ValidateExecutionLogging(this IReadOnlyList<ExecutionLog> expected,
        IReadOnlyList<ExecutionLogEntry> actual)
    {
        var errors = new List<string>();
        ValidateEntries(0, errors, actual, expected);
        return errors.Count > 0 ? errors : null;
    }

    private static void AddLog(int indent, ICollection<string> errors, string message)
        => errors.Add(new string(' ', indent * 2) + message);

    private static void ValidateEntries(int indent, IList<string> errors,
        IReadOnlyList<ExecutionLogEntry> actual, IReadOnlyList<ExecutionLog> expected)
    {
        var max = Math.Max(actual.Count, expected.Count);
        for (var index = 0; index < max; index++)
        {
            var actualEntry = index < actual.Count ? actual[index] : null;
            var expectedEntry = index < expected.Count ? expected[index] : null;
            ValidateEntry(indent, errors, actualEntry, expectedEntry);
        }
    }

    private static void ValidateEntry(int indent, IList<string> errors, ExecutionLogEntry actualEntry, ExecutionLog expectedEntry)
    {
        if (actualEntry == null && expectedEntry != null)
        {
            AddLog(indent, errors, $"{expectedEntry.Reference} - Not found: '{expectedEntry.Message}'");
        }
        else if (actualEntry != null && expectedEntry == null)
        {
            AddLog(indent, errors, $"Log not expected: '{actualEntry.Message}'");
        }
        else
        {
            ValidateEntryMessage(indent, errors, actualEntry, expectedEntry);
            ValidateEntries(indent + 1, errors, actualEntry.Entries, expectedEntry.Entries);
            ValidateVariables(indent, errors, false,
                actualEntry.ReadVariables, actualEntry.WriteVariables,
                expectedEntry.Assignments);
        }
    }

    private static void ValidateEntryMessage(int indent, IList<string> errors, ExecutionLogEntry actualEntry,
        ExecutionLog expectedEntry)
    {
        if (actualEntry.Message != expectedEntry.Message)
        {
            AddLog(indent, errors, "Invalid log:");
            AddLog(indent, errors, $"  Expected: {expectedEntry.Message}");
            AddLog(indent, errors, $"  Actual: {actualEntry.Message}");
        }
    }

    private static string GetName(IAssignmentDefinition expectedVariable, bool nested)
    {
        if (expectedVariable is ObjectAssignmentDefinition objectAssignment)
        {
            return nested ? objectAssignment.Variable.LastPart() : objectAssignment.Variable.FullPath();
        }

        var assignmentDefinition = expectedVariable as AssignmentDefinition;
        return nested ? assignmentDefinition?.Variable.LastPart() : assignmentDefinition?.Variable.FullPath();
    }

    private static bool Equal(object expectedValue, LogVariable actualValue)
    {
        if (actualValue.Type == LogVariableType.Enum)
        {
            var originalEnumType = actualValue.Value.GetType().Name.Substring(LexyCodeConstants.EnumClassPrefix.Length);
            var actualEnumValue = $"{originalEnumType}.{actualValue.Value}";
            return expectedValue.Equals(actualEnumValue);
        }
        return expectedValue.Equals(actualValue.Value);
    }

    private static void ValidateValueVariable(int indent, IList<string> errors, LogVariable actualValue,
        IAssignmentDefinition expectedVariable)
    {
        var definition = expectedVariable as AssignmentDefinition;
        var expected = definition?.Variable.FullPath();
        var expectedValue = definition?.ConstantValue.Value;
        if (!Equal(expectedValue, actualValue))
        {
            AddLog(indent, errors, $"{expectedVariable.Reference} - Invalid variable value: '{expected}'");
            AddLog(indent, errors, $"  Expected: '{expectedValue}'");
            AddLog(indent, errors, $"  Actual: '{actualValue}'");
        }
    }

    private static LogVariable GetActualValue(string name, LogVariables actualParameters, LogVariables actualResults)
    {
        if (name == null) return null;
        if (actualParameters != null && actualParameters.Contains(name))
        {
            return actualParameters[name];
        }

        return actualResults != null && actualResults.Contains(name) ? actualResults[name] : null;
    }

    private static void ValidateVariables(int indent, IList<string> errors, bool nested, LogVariables actualParameters,
        LogVariables actualResults,
        IReadOnlyList<IAssignmentDefinition> expectedEntries)
    {
        for (var index = 0; index < expectedEntries.Count(); index++)
        {
            var expectedVariable = expectedEntries[index];
            var name = GetName(expectedVariable, nested);
            var actualVariable = GetActualValue(name, actualParameters, actualResults);
            if (actualVariable == null)
            {
                AddLog(indent, errors, $"{expectedVariable.Reference} - Variable not found: '{name}'");
            }
            else
            {
                ValidateVariable(indent, errors, actualVariable, expectedVariable);
            }
        }
    }

    private static void ValidateVariable(int indent, IList<string> errors,
        LogVariable actualValue, IAssignmentDefinition expectedVariable)
    {
        if (expectedVariable is ObjectAssignmentDefinition objectAssignment)
        {
            ValidateVariables(indent, errors, true, actualValue.Value as LogVariables, null, objectAssignment.Assignments);
        }
        else
        {
            ValidateValueVariable(indent, errors, actualValue, expectedVariable);
        }
    }
}