
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem;

public abstract class Type
{
    public abstract bool IsAssignableFrom(Type type);

    public abstract Symbol GetSymbol(SourceReference reference);
}
