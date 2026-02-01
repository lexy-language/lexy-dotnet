using System.Text;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class Signature
{
    public string Name { get; }
    public SignatureParameter[] Parameters { get; }

    public Signature(string name, SignatureParameter[] parameters)
    {
        Name = Assert.NotNull(name, nameof(name));
        Parameters = Assert.NotNull(parameters, nameof(parameters));
    }

    public override string ToString()
    {
        var builder = new StringBuilder(Name + ": ");
        foreach (var parameter in Parameters)
        {
            builder.Append(parameter);
        }
        return builder.ToString();
    }
}
