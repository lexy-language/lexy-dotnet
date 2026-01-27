using System;
using System.Collections.Generic;
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
    public record SymbolsResult(DocumentsSymbols Symbols, ComponentNodeList nodes);

    public static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider,
        string fileName, string content)
    {
        var lines = content.Split("\n");
        return await serviceProvider.GetSymbols(new [] {new StringSourceCodeDocument(lines, fileName)});
    }

    public static async Task<SymbolsResult> GetSymbols(this IServiceProvider serviceProvider, IEnumerable<ISourceCodeDocument> documents)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var parser = serviceProvider.GetRequiredService<ILexyParser>();

        var context = await parser.ParseDocuments(documents, new ParseOptions {SuppressException = false});

        return new SymbolsResult(context.DocumentsSymbols, context.Nodes);
    }
}
