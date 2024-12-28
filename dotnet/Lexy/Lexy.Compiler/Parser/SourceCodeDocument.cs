using System;
using System.Linq;

namespace Lexy.Compiler.Parser;

public class SourceCodeDocument : ISourceCodeDocument
{
    private Line[] code;
    private int index;

    public SourceFile File { get; private set; }

    public Line CurrentLine { get; private set; }

    public void SetCode(string[] lines, string fileName)
    {
        index = -1;
        code = lines.Select((line, index) => new Line(index, line)).ToArray();
        File = new SourceFile(fileName);
    }

    public bool HasMoreLines()
    {
        return index < code.Length - 1;
    }

    public Line NextLine()
    {
        if (index >= code.Length) throw new InvalidOperationException("No more lines");

        CurrentLine = code[++index];
        return CurrentLine;
    }

    public void Reset()
    {
        CurrentLine = null;
    }
}