using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Documents;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Symbols;

public static class SymbolsExtensions
{
    public record SymbolsResult(DocumentsSymbols Symbols, ComponentNodeList Nodes);

    public static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider, string fileName,
        string content, bool suppressException = false)
    {
        var lines = content.Split("\n");
        var documents = new [] {new StringSourceCodeDocument(lines, fileName)};

        return await serviceProvider.GetSymbols(documents, suppressException);
    }

    private static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider,
        IEnumerable<ISourceCodeDocument> documents,
        bool suppressException = false)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var parser = serviceProvider.GetRequiredService<ILexyParser>();
        var options = new ParseOptions {SuppressException = suppressException};

        try
        {
            var context = await parser.ParseDocuments(documents, options);
            return new SymbolsResult(context.DocumentsSymbols, context.Nodes);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Parser error: " + LogDocuments(documents), exception);
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
