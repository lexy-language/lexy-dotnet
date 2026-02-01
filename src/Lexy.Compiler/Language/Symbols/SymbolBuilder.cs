using System;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Symbols;

public class SymbolBuilder
{
    public class SignatureContext
    {
        public SignatureContext Parameter(string name, string documentation)
        {
            throw new NotImplementedException();
        }
    }

    public class SignaturesContext
    {
        public SignaturesContext Signature(string name, Action<SignatureContext> build)
        {
            throw new NotImplementedException();
        }
    }

    private string name;
    private string description;
    private SymbolKind kind;
    private Signatures signatures;
    private SourceReference reference;

    public SymbolBuilder Reference(SourceReference reference)
    {
        this.reference = reference;
        return this;
    }

    public SymbolBuilder Name(string name)
    {
        this.name = name;
        return this;
    }

    public SymbolBuilder Description(string description)
    {
        this.description = description;
        return this;
    }

    public SymbolBuilder Kind(SymbolKind kind)
    {
        this.kind = kind;
        return this;
    }

    public SymbolBuilder Signatures(Action<SignaturesContext> build)
    {
        return this;
    }


    private Symbol CreateSymbol()
    {
        return new Symbol(reference, name, description, kind, signatures);
    }

    public static Symbol Build(Action<SymbolBuilder> handler)
    {
        var builder = new SymbolBuilder();
        handler(builder);
        return builder.CreateSymbol();
    }
}
