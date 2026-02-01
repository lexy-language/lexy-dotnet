using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language;

public interface INode
{
    SourceReference Reference { get; }
    SourceArea Area { get; }

    INode Parent { get; }

    void ValidateTree(IValidationContext context);

    IEnumerable<INode> GetChildren();

    Symbol GetSymbol();

    SuggestionEdit[] GetSuggestions();

    void ExpandArea(Position position);
}
