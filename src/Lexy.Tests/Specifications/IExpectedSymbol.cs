using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Tests.Specifications;

internal interface IExpectedSymbol
{
    bool Verify(IDocumentSymbols symbols, VerifyContext context);
}
