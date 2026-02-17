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
        if (Value is DateTime dateTime)
        {
            return DateTimeLiteralToken.FormatDate(dateTime);
        }
        return Value != null ? Value.ToString() : "null";
    }
}
