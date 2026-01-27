using System;
using System.Collections.Generic;

namespace Lexy.Compiler.Parser.Symbols;

public class DocumentsSymbols
{
    private readonly Dictionary<string, DocumentSymbols> symbols = new();

    public SymbolDescription GetDescription(string fileName, Position position)
    {
        var document = GetDocumentSymbols(fileName);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {fileName}");
        }

        return document.GetDescription(position);
    }

    public Signatures GetSignatures(string fileName, Position position)
    {
        var document = GetDocumentSymbols(fileName);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {fileName}");
        }

        return document.GetSignatures(position);
    }

    public Suggestions GetSuggestions(string fileName, Position position)
    {
        var document = GetDocumentSymbols(fileName);
        if (document == null)
        {
            throw new InvalidOperationException($"Couldn't find document: {fileName}");
        }

        var result = new List<Suggestion>();
        result.AddRange(AddLocalVariables(document, position));
        result.AddRange(AddComponentsAndMembers(document, position));
        result.AddRange(AddLibraryFunctions(document, position));
        result.AddRange(AddKeywords(document, position));
        return new Suggestions(result);
    }

    private IEnumerable<Suggestion> AddLocalVariables(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Suggestion> AddComponentsAndMembers(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Suggestion> AddLibraryFunctions(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Suggestion> AddKeywords(DocumentSymbols document, Position position)
    {
        throw new NotImplementedException();
    }

    private DocumentSymbols GetDocumentSymbols(string fileName)
    {
        return symbols.TryGetValue(fileName, out var value) ? value : null;
    }

    public DocumentSymbols Document(string fullFileName)
    {
        if (symbols.TryGetValue(fullFileName, out var value))
        {
            return value;
        }

        var documentSymbols = new DocumentSymbols();
        symbols.Add(fullFileName, documentSymbols);
        return documentSymbols;
    }
}
