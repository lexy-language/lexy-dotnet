using System;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public class SourceReference
{
    public int? CharacterNumber { get; }
    public int? LineNumber { get; }

    public SourceFile File { get; }

    public string SortIndex {
        get
        {
            var value = (LineNumber * 100000000 + CharacterNumber).ToString().PadLeft(16);
            return $"{File.FileName}/{value}";
        }
    }

    public SourceReference(SourceFile file, int? lineNumber, int? characterNumber)
    {
        File = Assert.NotNull(file, nameof(file));
        CharacterNumber = characterNumber;
        LineNumber = lineNumber;
    }

    public override string ToString()
    {
        return $"{File.FileName}({LineNumber}, {CharacterNumber})";
    }
}