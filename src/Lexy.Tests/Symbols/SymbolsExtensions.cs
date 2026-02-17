using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Documents;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Symbols;

public static class SymbolsExtensions
{
    public record SymbolsResult(ISymbols Symbols, IDocumentSymbols DocumentSymbols, ComponentNodeList Nodes, IFile File);

    public static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider, string fileName,
        string content, bool suppressException = false)
    {
        var lines = content.Split("\n");
        return await serviceProvider.GetSymbols(fileName, lines, suppressException);
    }

    public static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider, IFile file, bool suppressException = false)
    {
        var filesystem = new FileSystem();
        var document = await filesystem.CreateFileSourceDocument(file);

        return await serviceProvider.GetSymbols(file.Project, document, suppressException);
    }

    private static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider, string fileName,
        string[] lines, bool suppressException = false)
    {
        var filesystem = new FileSystem();
        var project = new Project(filesystem);
        var file = project.File(fileName);
        var document = new StringSourceCodeDocument(file, lines);

        return await serviceProvider.GetSymbols(project, document, suppressException);
    }

    private static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider,
        IProject project,
        ISourceCodeDocument document,
        bool suppressException = false)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var parser = serviceProvider.GetRequiredService<ILexyParser>();
        var options = new ParseOptions {SuppressException = suppressException};
        var documents = new[] { document };

        try
        {
            var context = await parser.ParseDocuments(project, documents, options);
            var documentSymbol = context.Symbols.Document(document.File);
            return new SymbolsResult(context.Symbols, documentSymbol, context.Nodes, document.File);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Parser error: \n" + LogDocuments(documents), exception);
        }
    }

    private static string LogDocuments(IEnumerable<ISourceCodeDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var document in documents)
        {
            builder.AppendLine(document.ToString());
        }
        return builder.ToString();
    }
}
