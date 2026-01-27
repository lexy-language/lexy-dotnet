using System.Collections.Generic;

namespace Lexy.Compiler.Parser.Symbols;

public class Suggestions
{
    private readonly List<Suggestion> result;

    public Suggestions(List<Suggestion> result)
    {
        this.result = result;
    }
}