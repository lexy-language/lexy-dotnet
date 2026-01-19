using System;

namespace Lexy.RunTime;

public class LogVariable
{
    public LogType Type { get; }
    public object Value { get; }

    public LogVariables LogVariables => Type == LogType.LogVariables ? Value as LogVariables : null;

    public LogVariable(object value, LogType type)
    {
        Value = value;
        Type = type;
    }

    public override string ToString()
    {
        return Value?.ToString();
    }
}
