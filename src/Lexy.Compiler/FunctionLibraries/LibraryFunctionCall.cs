using Lexy.Compiler.Language;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Functions;

namespace Lexy.Compiler.FunctionLibraries;

internal class LibraryFunctionCall : IMemberFunctionCall
{
    public IdentifierPath FullTypeName { get; }
    public Type ReturnType { get; }

    public LibraryFunctionCall(IdentifierPath fullTypeName, Type returnType)
    {
        FullTypeName = fullTypeName;
        ReturnType = returnType;
    }
}