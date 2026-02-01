using Lexy.RunTime;

namespace Lexy.Compiler.Language.Symbols;

public class SignatureParameter
{
    public string Name { get; }
    public string Documentation { get; }

    public SignatureParameter(string name, string documentation)
    {
        Name = Assert.NotNull(name, nameof(name));
        Documentation = Assert.NotNull(documentation, nameof(documentation));
    }

    public override string ToString()
    {
        return $"{Name}: {Documentation}";
    }
}
