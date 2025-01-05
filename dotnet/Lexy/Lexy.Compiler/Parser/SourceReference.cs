using System;

namespace Lexy.Compiler.Parser;

public class SourceReference
{
    public int? CharacterNumber { get; }
    public int? LineNumber { get; }

    public SourceFile File { get; }

    public SourceReference(SourceFile file, int? lineNumber, int? characterNumber)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        this.CharacterNumber = characterNumber;
        this.LineNumber = lineNumber;
    }

    public override string ToString()
    {
        return $"{File.FileName}({LineNumber}, {CharacterNumber})";
    }
}