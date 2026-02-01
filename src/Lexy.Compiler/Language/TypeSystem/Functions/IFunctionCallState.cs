using Lexy.Compiler.Language.Symbols;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

public interface IFunctionCallState
{
    Symbol GetSymbol();
}
