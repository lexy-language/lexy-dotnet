using System.Text;

namespace Lexy.Compiler.Parser.Symbols;

public class Signature
{
    public string Name { get; }
    public SignatureParameter[] Parameters { get; }

    public Signature(string name, SignatureParameter[] parameters)
    {
        Name = name;
        Parameters = parameters;
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
