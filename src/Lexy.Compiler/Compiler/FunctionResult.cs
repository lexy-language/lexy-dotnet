using System;
using System.Reflection;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

public class FunctionResult
{
    private readonly object valueObject;

    public FunctionResult(object valueObject)
    {
        this.valueObject = valueObject;
    }

    public decimal Number(string name)
    {
        var value = GetValue(VariableReference.Parse(name));
        return (decimal)value;
    }

    private FieldInfo GetField(object parentObject, string name)
    {
        var type = parentObject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null) throw new InvalidOperationException($"Couldn't find field: '{name}' on type: '{type.Name}'");
        return field;
    }

    public object GetValue(string value)
    {
        return GetValue(VariableReference.Parse(value));
    }

    public object GetValue(VariableReference expectedVariable)
    {
        var currentReference = expectedVariable;
        var currentValue = GetField(valueObject, expectedVariable.ParentIdentifier).GetValue(valueObject);
        while (currentReference.HasChildIdentifiers)
        {
            currentReference = currentReference.ChildrenReference();
            currentValue = GetField(currentValue, currentReference.ParentIdentifier).GetValue(currentValue);
        }

        return currentValue;
    }
}