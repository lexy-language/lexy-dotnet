using System;

namespace Lexy.Compiler.Language.Tables;

public class TableName
{
    public string Value { get; private set; }

    public void ParseName(string parameter)
    {
        Value = parameter;
    }
}