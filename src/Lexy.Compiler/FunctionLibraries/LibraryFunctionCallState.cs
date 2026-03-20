using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Functions;

namespace Lexy.Compiler.FunctionLibraries;

internal class LibraryFunctionCallState : IFunctionCallState
{
    public IdentifierPath FullTypeName { get; }
    public IdentifierPath IdentifierPath { get; }
    public Type ReturnType { get; }
    public SourceReference Reference { get; }

    public LibraryFunctionCallState(SourceReference reference, IdentifierPath fullTypeName,
        IdentifierPath identifierPath, Type returnType)
    {
        Reference = reference;
        FullTypeName = fullTypeName;
        IdentifierPath = identifierPath;
        ReturnType = returnType;
    }

    public Symbol GetSymbol() => new(Reference, IdentifierPath.FullPath(), string.Empty, SymbolKind.LibraryFunction);
}
