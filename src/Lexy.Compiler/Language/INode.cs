using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language;

public interface INode
{
    SourceReference Reference { get; }

    void ValidateTree(IValidationContext context);

    IEnumerable<INode> GetChildren();

    Symbol GetSymbol();
}
