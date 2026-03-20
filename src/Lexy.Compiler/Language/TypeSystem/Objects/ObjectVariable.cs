namespace Lexy.Compiler.Language.TypeSystem.Objects;

public class ObjectVariable : IObjectMember
{
    public string Name { get; }
    public Type Type { get; }

    public ObjectVariable(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    public string Description()
    {
        return $"variable: {Type}";
    }
}
