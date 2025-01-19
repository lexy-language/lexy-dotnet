using System;
using System.Linq;
using System.Text;

namespace Lexy.Compiler.Language;

public class VariableReference
{
    public string[] Path { get; }
    public string ParentIdentifier => Path[0];
    public bool HasChildIdentifiers => Path.Length > 1;
    public int Parts => Path.Length;

    public VariableReference(string[] variablePath)
    {
        Path = variablePath ?? throw new ArgumentNullException(nameof(variablePath));
    }

    public string FullPath() => string.Join(".", Path);
    public override string ToString() => FullPath();

    public VariableReference ChildrenReference()
    {
        var parts = Path[1..];
        return new VariableReference(parts);
    }

    public static VariableReference Parse(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        var parts = name.Split(".");
        return new VariableReference(parts);
    }

    public VariableReference Append(string[] path)
    {
        var newPath = Path.Concat(path).ToArray();
        return new VariableReference(newPath);
    }
}