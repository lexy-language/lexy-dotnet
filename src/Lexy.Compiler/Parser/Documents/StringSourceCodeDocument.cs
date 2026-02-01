using System;
using System.Linq;
using System.Text;

namespace Lexy.Compiler.Parser.Documents;

public class StringSourceCodeDocument : ISourceCodeDocument
{
    private readonly Line[] code;
    private readonly string fileName;

    private int index;

    public string FullFileName => fileName;
    public Line CurrentLine { get; private set; }

    public StringSourceCodeDocument(string[] code, string fileName)
    {
        index = -1;
        this.fileName = fileName;
        this.code = code.Select((line, index) => new Line(index, line, fileName)).ToArray();
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

    public override string ToString()
    {
        var sourceCode = new StringBuilder();
        foreach (var line in code)
        {
            sourceCode.AppendLine(line.ToString());
        }
        return "Code: " + sourceCode;
    }
}
