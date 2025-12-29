using System;
using System.Linq;
using System.Text;

namespace Lexy.Compiler.Language;

public class IdentifierPath
{
    public string[] Path { get; }
    public string RootIdentifier => Path[0];
    public bool HasChildIdentifiers => Path.Length > 1;
    public int Parts => Path.Length;

    public IdentifierPath(params string[] path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public string FullPath() => string.Join(".", Path);
    public override string ToString() => FullPath();

    public IdentifierPath ChildrenReference()
    {
        var parts = Path[1..];
        return new IdentifierPath(parts);
    }

    public IdentifierPath Append(string[] path)
    {
        var newPath = Path.Concat(path).ToArray();
        return new IdentifierPath(newPath);
    }

    public string LastPart()
    {
        return Path[^1];
    }

    public IdentifierPath WithoutLastPart()
    {
        if (Parts <= 1) throw new InvalidOperationException("Parts <= 1");

        return new IdentifierPath(Path.Take(Parts - 1).ToArray());
    }

    public static IdentifierPath Parse(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        var parts = SplitBySeparator(name);
        return new IdentifierPath(parts);
    }

    public static IdentifierPath Parse(params string[] parts)
    {
        var parsePaths = parts.SelectMany(SplitBySeparator).ToArray();
        return new IdentifierPath(parsePaths);
    }

    private static string[] SplitBySeparator(string name) => name.Split(".");
}