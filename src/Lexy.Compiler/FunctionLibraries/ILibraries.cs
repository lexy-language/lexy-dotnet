using Lexy.Compiler.Language;

namespace Lexy.Compiler.FunctionLibraries;

public interface ILibraries
{
    ILibrary GetLibrary(IdentifierPath identifier);
}