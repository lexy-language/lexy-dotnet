namespace Lexy.Compiler.Parser.Symbols;

public class SignatureParameter
{
    public string Name { get; }
    public string Documentation { get; }

    public SignatureParameter(string name, string documentation)
    {
        Name = name;
        Documentation = documentation;
    }

    public override string ToString()
    {
        return $"{Name}: {Documentation}";
    }
}
