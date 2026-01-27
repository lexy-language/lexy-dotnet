using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Documents;

public class FileDocuments : IDisposable
{
    private readonly FileSourceDocument[] documents;

    public ISourceCodeDocument[] Documents => documents;

    private FileDocuments(FileSourceDocument[] documents)
    {
        this.documents = Assert.NotNull(documents, nameof(documents));
    }

    public static FileDocuments Create(IFileSystem fileSystem, IEnumerable<string> fileNames)
    {
        var documents = fileNames.Select(fileName =>
        {
            var fullPath = fileSystem.GetFullPath(fileName);
            return new FileSourceDocument(fullPath);
        }).ToArray();

        return new FileDocuments(documents);
    }

    public void Dispose()
    {
        var exceptions = new List<Exception>();
        foreach (var document in documents)
        {
            try
            {
                document.Dispose();
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Error occurred while disposing source file documents.", exceptions);
        }
    }
}