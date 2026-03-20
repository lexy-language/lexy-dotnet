using System;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class ConstantValue
{
    public object Value { get; }

    public ConstantValue(object value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value switch
        {
            DateTime dateTime => DateTimeLiteralToken.FormatDate(dateTime),
            bool boolean => boolean.ToString().ToLowerInvariant(),
            _ => Value != null ? Value.ToString() : "null"
        };
    }
}
