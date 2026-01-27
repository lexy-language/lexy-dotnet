using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

public interface IFunctionCallState
{
    Symbol GetSymbol();
}
