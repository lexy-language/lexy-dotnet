using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language;

public class VariableEntry
{
    public string Name { get; }
    public Type Type { get; }
    public VariableSource VariableSource { get; }
    public SourceReference Reference { get; }

    public VariableEntry(string name, Type type, VariableSource variableSource, SourceReference reference = null)
    {
        Name = name;
        Type = type;
        VariableSource = variableSource;
        Reference = reference;
    }
}
