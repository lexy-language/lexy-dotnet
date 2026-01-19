using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lexy.RunTime;

public class LogVariablesBuilder
{
    private IDictionary<string, LogVariable> variables = new Dictionary<string, LogVariable>();

    public LogVariablesBuilder AddVariable(string name, object value)
    {
        variables.Add(name, CreateVariable(value));
        return this;
    }

    private LogVariable CreateVariable(object value)
    {
        switch (value)
        {
            case DateTime:
                return new LogVariable(value, LogType.Date);
            case decimal:
                return new LogVariable(value, LogType.Number);
            case bool:
                return new LogVariable(value, LogType.Boolean);
            case string:
                return new LogVariable(value, LogType.String);
        }

        if (value.GetType().IsEnum)
        {
            return new LogVariable(value, LogType.Enum);
        }
        if (value.GetType().IsValueType)
        {
            throw new InvalidOperationException($"Invalid variable type: '{value.GetType().Name}'");
        }
        return new LogVariable(CreateVariables(value), LogType.LogVariables);
    }

    private LogVariables CreateVariables(object value)
    {
        var properties = value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var variables = properties
            .ToDictionary<PropertyInfo, string, LogVariable>(property =>
                    property.Name,
                property => CreateVariable(property.GetValue(value)));

        var fields = value.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            variables.Add(field.Name, CreateVariable(field.GetValue(value)));
        }

        return new LogVariables(variables);
    }

    public static LogVariablesBuilder New()
    {
        return new LogVariablesBuilder();
    }

    public LogVariables Build() => new LogVariables(variables);
}
