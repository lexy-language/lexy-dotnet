using System.Collections.Generic;

namespace Lexy.Compiler.Language.TypeSystem;

public static class TypeNames
{
    public const string Number = "number";
    public const string Boolean = "boolean";
    public const string Date = "date";
    public const string String = "string";

    public static readonly IList<string> Values = new List<string>
    {
        Number,
        Boolean,
        Date,
        String
    };

    public static bool Contains(string parameterType)
    {
        return Values.Contains(parameterType);
    }
}
