using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Documents;

public class FileSourceDocuments : ISourceCodeDocuments
{
    private readonly FileSourceDocument[] documents;

    public IEnumerable<ISourceCodeDocument> Documents => documents;

    private FileSourceDocuments(FileSourceDocument[] documents)
    {
        this.documents = Assert.NotNull(documents, nameof(documents));
    }

    public static FileSourceDocuments Create(IFileSystem fileSystem, IEnumerable<IFile> files)
    {
        var documents = files.Select(file => new FileSourceDocument(fileSystem, file)).ToArray();

        return new FileSourceDocuments(documents);
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
