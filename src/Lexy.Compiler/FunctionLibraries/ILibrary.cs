using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.FunctionLibraries;

public interface ILibrary
{
    IInstanceFunction GetFunction(string identifier);
}