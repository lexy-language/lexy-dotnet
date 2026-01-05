namespace Lexy.Compiler.Language.VariableTypes;

public class ComplexTypeVariable : IComplexTypeVariable
{
    public string Name { get; }
    public VariableType Type { get; }

    public ComplexTypeVariable(string name, VariableType type)
    {
        Name = name;
        Type = type;
    }
}