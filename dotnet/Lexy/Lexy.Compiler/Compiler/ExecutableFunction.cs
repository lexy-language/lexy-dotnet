using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.Compiler;

public class ExecutableFunction
{
    private record ParameterSetter(Type FieldType, Action<object> SetValue);

    private readonly Type parametersType;

    private readonly MethodInfo runMethod;
    private readonly IDictionary<string, ParameterSetter> parameterFields = new Dictionary<string, ParameterSetter>();

    public ExecutableFunction(Type functionType)
    {
        runMethod = functionType.GetMethod(LexyCodeConstants.RunMethod, BindingFlags.Static | BindingFlags.Public);
        parametersType = functionType.GetNestedType(LexyCodeConstants.ParametersType);
    }

    public FunctionResult Run(IExecutionContext context, IDictionary<string, object> values = null)
    {
        values ??= new Dictionary<string, object>();
        var parameters = CreateParameters();

        foreach (var value in values)
        {
            var field = GetParameterSetter(parameters, value.Key);
            var convertedValue = Convert.ChangeType(value.Value, field.FieldType);
            field.SetValue(convertedValue);
        }

        var results = runMethod.Invoke(null, new[] { parameters, context });

        return new FunctionResult(results);
    }

    private object CreateParameters()
    {
        return Activator.CreateInstance(parametersType);
    }

    private ParameterSetter GetParameterSetter(object parameters, string name)
    {
        if (parameterFields.ContainsKey(name)) return parameterFields[name];

        var currentReference = VariableReference.Parse(name);
        var currentValue = parameters;
        var field = GetField(currentReference.ParentIdentifier, parameters);

        while (currentReference.HasChildIdentifiers)
        {
            currentReference = currentReference.ChildrenReference();
            currentValue = field.GetValue(currentValue);
            field = GetField(currentReference.ParentIdentifier, currentValue);
        }

        var setter = new ParameterSetter(field.FieldType, (value) => field.SetValue(currentValue, value));
        parameterFields[name] = setter;
        return setter;
    }

    private static FieldInfo? GetField(string name, object valueObject)
    {
        if (valueObject == null) throw new ArgumentNullException(nameof(valueObject));

        var type = valueObject.GetType();
        var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field == null)
        {
            throw new InvalidOperationException($"Couldn't find parameter field: '{name}' on type: '{type.Name}'");
        }

        return field;
    }
}