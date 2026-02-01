using System.Collections.Generic;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Context;

public class ParserContext : IParserContext
{
    private readonly IList<string> includedFiles = new List<string>();

    public ILibraries Libraries { get; }

    public ComponentNodeList Nodes => RootNode.ComponentNodes;
    public ILineFilter LineFilter { get; private set; }

    public LexyScriptNode RootNode { get; }
    public IParserLogger Logger { get; }
    public ParseOptions Options { get; }

    public IFileSystem FileSystem { get; }
    public DocumentsSymbols Symbols { get; }

    public ParserContext(IParserLogger logger, IFileSystem fileSystem, ILibraries libraries, ParseOptions options)
    {
        FileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        Logger = Assert.NotNull(logger, nameof(logger));
        Libraries = Assert.NotNull(libraries, nameof(libraries));

        Options = options ?? ParseOptions.Default();

        RootNode = new LexyScriptNode();
        LineFilter = new DefaultLineFilter();
        Symbols = new DocumentsSymbols(RootNode);
    }

    public void AddFileIncluded(string fileName)
    {
        var path = NormalizePath(fileName);

        includedFiles.Add(path);
    }

    public bool IsFileIncluded(string fileName)
    {
        return includedFiles.Contains(NormalizePath(fileName));
    }

    private string NormalizePath(string fileName)
    {
        return FileSystem.GetFullPath(fileName);
    }

    public void SetFileLineFilter(string fileName)
    {
        LineFilter = fileName.EndsWith(LexySourceDocument.MarkdownExtension) ? new MarkdownLineFilter() : new DefaultLineFilter();
    }
}
