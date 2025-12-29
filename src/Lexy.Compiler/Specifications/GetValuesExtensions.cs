using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;

namespace Lexy.Compiler.Specifications;

public static class GetValuesExtensions
{
    public static IDictionary<string, object> GetValues(this Parameters parameters)
    {
        var result = new Dictionary<string, object>();
        if (parameters == null) return result;

        foreach (var parameter in parameters.AllAssignments())
        {
            SetParametersValue(parameter, result);
        }

        return result;
    }

    private static void SetParametersValue(AssignmentDefinition parameter,
        Dictionary<string, object> result)
    {
        SetValueObjectProperty(result, parameter.Variable, parameter.ConstantValue.Value);
    }

    public static IDictionary<string, object> GetValues(this ValidationTableRow row,
        Function functionNode, ValidationTableHeader header)
    {
        var result = new Dictionary<string, object>();
        SetRowParameters(functionNode, header, row, result);
        return result;
    }

    public static void SetRowParameters(Function functionNode,
        ValidationTableHeader header,
        ValidationTableRow row,
        IDictionary<string, object> result)
    {
        for (var index = 0; index < row.Values.Count; index++)
        {
            var column = header.GetColumn(index);
            var value = row.Values[index];
            if (column == null) continue;
            SetRowParameter(functionNode, column, value, result);
        }
    }

    public static void SetRowParameter(Function functionNode,
        ValidationColumnHeader column,
        ValidationTableValue value,
        IDictionary<string, object> result)
    {
        var variableReference = IdentifierPath.Parse(column.Name);
        if (!IsParameter(functionNode, variableReference)) return;
        SetValueObjectProperty(result, variableReference, value.GetValue());
    }

    private static void SetValueObjectProperty(IDictionary<string, object> result,
        IdentifierPath variableReference,
        object value)
    {
        var reference = variableReference;
        var valueObject = result;
        while (reference.HasChildIdentifiers)
        {
            if (!valueObject.ContainsKey(reference.RootIdentifier))
            {
                var childObject = new Dictionary<string, object>();
                valueObject.Add(reference.RootIdentifier, childObject);
            }

            if (valueObject[reference.RootIdentifier] is not Dictionary<string, object> dictionary)
            {
                throw new InvalidOperationException(
                    $"Parent variable '{reference.RootIdentifier}' of parameter '{variableReference}' already set to value: {valueObject[reference.RootIdentifier].GetType()}");
            }

            valueObject = dictionary;
            reference = reference.ChildrenReference();
        }

        valueObject.Add(reference.RootIdentifier, value);
    }

    private  static bool IsParameter(Function functionNode, IdentifierPath path)
    {
        if (functionNode?.Parameters == null) return false;
        return functionNode.Parameters.Variables.Any(parameter => parameter.Name == path.RootIdentifier);
    }
}
