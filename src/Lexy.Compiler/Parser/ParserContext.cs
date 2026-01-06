using System.Collections.Generic;
using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public class ParserContext : IParserContext
{
    private readonly IFileSystem fileSystem;

    private readonly IList<string> includedFiles = new List<string>();

    public ILibraries Libraries { get; }

    public ComponentNodeList Nodes => RootNode.ComponentNodes;
    public ILineFilter LineFilter { get; private set; }

    public LexyScriptNode RootNode { get; }
    public IParserLogger Logger { get; }
    public ParseOptions Options { get; }

    public ParserContext(IParserLogger logger, IFileSystem fileSystem, ILibraries libraries, ParseOptions options)
    {
        this.fileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        Logger = Assert.NotNull(logger, nameof(logger));
        Libraries = Assert.NotNull(libraries, nameof(libraries));

        Options = options ?? ParseOptions.Default();

        RootNode = new LexyScriptNode();
        LineFilter = new DefaultLineFilter();
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
        return fileSystem.GetFullPath(fileName);
    }

    public void SetFileLineFilter(string fileName)
    {
        LineFilter = fileName.EndsWith(LexySourceDocument.MarkdownExtension) ? new MarkdownLineFilter() : new DefaultLineFilter();
    }
}