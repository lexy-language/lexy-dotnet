using System;

namespace Lexy.Compiler.Language.Types;

public class TypeName
{
    public string Value { get; private set; } = Guid.NewGuid().ToString("D");

    public void ParseName(string parameter)
    {
        Value = parameter;
    }
}