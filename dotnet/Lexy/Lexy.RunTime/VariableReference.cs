using System;
using System.Text;

namespace Lexy.RunTime;

public class VariableReference
{
    public string[] Path { get; }
    public string ParentIdentifier => Path[0];
    public bool HasChildIdentifiers => Path.Length > 1;
    public int Parts => Path.Length;

    public VariableReference(string variableName)
    {
        if (variableName == null) throw new ArgumentNullException(nameof(variableName));
        Path = new[] { variableName };
    }

    public VariableReference(string[] variablePath)
    {
        Path = variablePath ?? throw new ArgumentNullException(nameof(variablePath));
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var value in Path)
        {
            if (builder.Length > 0) builder.Append('.');
            builder.Append(value);
        }

        return builder.ToString();
    }

    public VariableReference ChildrenReference()
    {
        var parts = Path[1..];
        return new VariableReference(parts);
    }
}