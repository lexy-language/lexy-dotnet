using System.Text;

namespace Lexy.Compiler.Parser.Symbols;

public class Signatures
{
    public Signature[] Values { get; }

    public Signatures(Signature[] values)
    {
        Values = values;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var signature in Values)
        {
            builder.AppendLine("- " + signature);
        }
        return builder.ToString();
    }
}
