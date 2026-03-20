namespace Lexy.Compiler.Language.TypeSystem.Objects;

public interface IObjectMember
{
    string Name { get; }
    Type Type { get; }

    string Description();
}
