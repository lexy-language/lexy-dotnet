using System;
using System.IO;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Documents;

internal class FileSourceDocument : ISourceCodeDocument, IDisposable
{
    private readonly string fileName;
    private StreamReader streamReader;

    private int index;

    public string FullFileName => fileName;

    public FileSourceDocument(string fileName)
    {
        this.fileName = fileName;
    }

    public bool HasMoreLines()
    {
        EnsureOpen();
        return !streamReader.EndOfStream;
    }

    private void EnsureOpen()
    {
        if (streamReader == null)
        {
            streamReader = File.OpenText(fileName);
        }
    }

    public Line NextLine()
    {
        Assert.NotNull(streamReader, nameof(streamReader));
        Assert.False(streamReader.EndOfStream, "No more lines.");

        var line = streamReader.ReadLine();
        return new Line(index++, line, fileName);
    }

    public void Dispose()
    {
        streamReader?.Dispose();
    }
}
