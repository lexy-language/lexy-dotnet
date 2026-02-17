using System;
using System.Text;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Documents;

public class StringSourceCodeDocument : ISourceCodeDocument
{
    private readonly string[] code;

    private int index;

    public IFile File { get; }

    public Line CurrentLine { get; private set; }

    public StringSourceCodeDocument(IFile file, string[] code)
    {
        index = 0;
        File = Assert.NotNull(file, nameof(file));
        this.code = Assert.NotNull(code, nameof(code));
    }

    public bool HasMoreLines()
    {
        return index <= code.Length - 1;
    }

    public Line NextLine()
    {
        if (index >= code.Length) throw new InvalidOperationException("No more lines");

        CurrentLine = CreateLine(index++);
        return CurrentLine;
    }

    public override string ToString()
    {
        var sourceCode = new StringBuilder();
        for (var lineIndex = 0; lineIndex < code.Length; lineIndex++)
        {
            sourceCode.AppendLine(CreateLine(index).ToString());
        }
        return "Code: " + sourceCode;
    }

    public void Dispose()
    {
    }

    private Line CreateLine(int lineIndex)
    {
        var lineContents = code[lineIndex];
        return new Line(lineIndex, lineContents, File);
    }
}
