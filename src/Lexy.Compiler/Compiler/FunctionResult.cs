using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.Compiler;

public class FunctionResult
{
    private readonly object valueObject;

    public IReadOnlyList<ExecutionLogEntry> Logging { get; }

    public FunctionResult(object valueObject, IReadOnlyList<ExecutionLogEntry> logging)
    {
        this.valueObject = valueObject;
        Logging = logging;
    }

    public decimal Number(string name)
    {
        var value = GetValue(IdentifierPath.Parse(name));
        return (decimal)value;
    }

    private FieldInfo GetField(object parentObject, string name)
    {
        var type = parentObject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null) throw new InvalidOperationException($"Couldn't find field: '{name}' on type: '{type.Name}'");
        return field;
    }

    public object GetValue(string value) => GetValue(IdentifierPath.Parse(value));

    public object GetValue(IdentifierPath expectedVariable)
    {
        var currentReference = expectedVariable;
        var currentValue = GetField(valueObject, expectedVariable.RootIdentifier).GetValue(valueObject);
        while (currentReference.HasChildIdentifiers)
        {
            currentReference = currentReference.ChildrenReference();
            currentValue = GetField(currentValue, currentReference.RootIdentifier).GetValue(currentValue);
        }

        return currentValue;
    }
}