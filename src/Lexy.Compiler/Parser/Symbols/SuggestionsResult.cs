using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Symbols;

public class SuggestionsResult
{
    public IReadOnlyList<Suggestion> Filtered { get; }
    public IReadOnlyList<Suggestion> All { get; }
    public string Filter { get; }

    public SuggestionsResult()
    {
        Filtered = new List<Suggestion>();
        All = new List<Suggestion>();
    }

    public SuggestionsResult(IReadOnlyList<Suggestion> filtered, IReadOnlyList<Suggestion> all, string filter)
    {
        Filtered = Assert.NotNull(filtered, nameof(filtered));
        All = Assert.NotNull(all, nameof(all));
        Filter = filter;
    }
}
