namespace Lexy.Compiler.Language.TypeSystem;

public class VoidType : Type
{
    public override bool IsAssignableFrom(Type type) => Equals(type);
}