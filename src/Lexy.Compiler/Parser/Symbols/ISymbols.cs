using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;

namespace Lexy.Compiler.Parser.Symbols;

public interface ISymbols
{
    SymbolDescription GetDescription(IFile file, Position position);
    Signatures GetSignatures(IFile file, Position position);
    SuggestionsResult GetSuggestions(IFile file, Position position);

    IDocumentSymbols Document(IFile fullFileName);

    void AddNodeVariables(INode node, IReadOnlyList<VariableEntry> result);
}