
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem;

public abstract class Type
{
    public abstract bool IsAssignableFrom(Type type);

    public abstract Symbol GetSymbol(SourceReference reference);
}
