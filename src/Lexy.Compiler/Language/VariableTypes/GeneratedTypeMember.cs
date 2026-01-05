namespace Lexy.Compiler.Language.VariableTypes;

public class GeneratedTypeMember
{
    public string Name { get; }
    public VariableType Type { get; }

    public GeneratedTypeMember(string name, VariableType type)
    {
        Name = name;
        Type = type;
    }
}