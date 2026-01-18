
namespace Lexy.Compiler.Language.TypeSystem;

public abstract class Type
{
    public abstract bool IsAssignableFrom(Type type);
}
