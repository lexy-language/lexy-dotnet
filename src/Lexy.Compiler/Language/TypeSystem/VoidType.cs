using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem;

public class VoidType : Type
{
    public override bool IsAssignableFrom(Type type) => Equals(type);

    public override string ToString()
    {
        return "void";
    }

    public override Symbol GetSymbol(SourceReference reference)
    {
        return new Symbol(reference, ToString(), string.Empty, SymbolKind.Type);
    }
}
