using System.Text;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class Signatures
{
    public Signature[] Values { get; }

    public Signatures(Signature[] values)
    {
        Values = Assert.NotNull(values, nameof(values));
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
