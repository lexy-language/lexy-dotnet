namespace Lexy.Compiler.Language.TypeSystem.Objects;

public class ObjectNestedType : IObjectMember
{
    public string Name { get; }
    public Type Type { get; }

    public ObjectNestedType(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}