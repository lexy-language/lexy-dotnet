using System;
using System.Reflection;

namespace Lexy.RunTime;

public class FunctionResult
{
    private readonly object valueObject;

    public FunctionResult(object valueObject)
    {
        this.valueObject = valueObject;
    }

    public decimal Number(string name)
    {
        var value = GetValue(new VariableReference(name));
        return (decimal)value;
    }

    private FieldInfo GetField(object parentbject, string name)
    {
        var type = parentbject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null) throw new InvalidOperationException($"Couldn't find field: '{name}' on type: '{type.Name}'");
        return field;
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