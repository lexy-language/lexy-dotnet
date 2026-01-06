namespace Lexy.Compiler.Language.VariableTypes;

public class ObjectTypeVariable : IObjectTypeVariable
{
    public string Name { get; }
    public VariableType Type { get; }

    public ObjectTypeVariable(string name, VariableType type)
    {
        Name = name;
        Type = type;
    }
}