using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;

namespace Lexy.Compiler.Parser.Symbols;

public interface IDocumentSymbols
{
    void Add(Line line);
    void Add(IComponentNode componentNode);

    void WalkSymbols(Action<INode, Symbol> symbolWalker);

    SymbolDescription GetDescription(Position position);

    IReadOnlyList<INode> GetNodesInScope(Position position);
}
