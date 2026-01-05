using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Functions;

namespace Lexy.Compiler.FunctionLibraries;

internal class LibraryFunctionCall : IMemberFunctionCall
{
    public IdentifierPath FullTypeName { get; }
    public VariableType ReturnType { get; }

    public LibraryFunctionCall(IdentifierPath fullTypeName, VariableType returnType)
    {
        FullTypeName = fullTypeName;
        ReturnType = returnType;
    }
}