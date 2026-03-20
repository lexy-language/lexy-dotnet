using System;
using System.Collections.Generic;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;

namespace Lexy.Tests.Specifications;

internal class CodeRange
{
    private static readonly Random random = new();

    private readonly Symbol symbol;
    private readonly List<int> options = new();
    private readonly int lineNumber;

    public CodeRange(Symbol symbol)
    {
        this.symbol = symbol;
        lineNumber = symbol.Reference.LineNumber;

        for (var index = symbol.Reference.Column; index <= symbol.Reference.EndColumn; index++)
        {
            options.Add(index);
        }
    }

    public int? Random()
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException("No reference options: line: " + lineNumber);
        }

        var index = random.Next(options.Count);
        return options[index];
    }

    public void Subtract(SourceReference reference)
    {
        if (reference.LineNumber != lineNumber) return;

        for (var index = reference.Column; index <= reference.EndColumn; index++)
        {
            var found = options.IndexOf(index);
            if (found >= 0)
            {
                options.RemoveAt(found);
            }
        }
    }
}