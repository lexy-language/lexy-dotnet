using System.Collections.Generic;

namespace Lexy.Compiler.Infrastructure;

internal class StringArrayBuilder
{
    private readonly IList<string> values = new List<string>();

    private StringArrayBuilder(string value)
    {
        Add(value);
    }

    public StringArrayBuilder Add(string value, int indent = 0)
    {
        var indentString = IndentString(indent);
        values.Add(indentString + value);
        return this;
    }

    private static string IndentString(int indent)
    {
        return indent > 0 ? new string(' ', indent) : string.Empty;
    }

    public StringArrayBuilder List(IEnumerable<string> strings)
    {
        var indentString = IndentString(2);
        foreach (var value in strings)
        {
            values.Add(indentString + value);
        }
        return this;
    }

    public IEnumerable<string> Array() => values;

    public static StringArrayBuilder New(string value)
    {
        return new StringArrayBuilder(value);
    }
}