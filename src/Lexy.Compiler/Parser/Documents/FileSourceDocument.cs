using System;
using System.IO;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;
using File = Lexy.Compiler.Infrastructure.File;

namespace Lexy.Compiler.Parser.Documents;

internal class FileSourceDocument : ISourceCodeDocument
{
    private readonly IFileSystem fileSystem;
    private StreamReader streamReader;

    private int index;

    public IFile File { get; }

    public FileSourceDocument(IFileSystem fileSystem, IFile file)
    {
        this.fileSystem = fileSystem;
        File = Assert.NotNull(file, nameof(file));
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
            streamReader = fileSystem.OpenStream(File.FullPath);
        }
    }

    public Line NextLine()
    {
        Assert.NotNull(streamReader, nameof(streamReader));
        Assert.False(streamReader.EndOfStream, "No more lines.");

        var line = streamReader.ReadLine();
        return new Line(index++, line, File);
    }

    public void Dispose()
    {
        streamReader?.Dispose();
    }
}
