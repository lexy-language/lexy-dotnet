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
    public ISymbols Symbols { get; }

    public IProject Project { get; }

    public ParserContext(IProject project, IParserLogger logger, IFileSystem fileSystem, ILibraries libraries, ParseOptions options)
    {
        Project = Assert.NotNull(project, nameof(project));

        FileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        Logger = Assert.NotNull(logger, nameof(logger));
        Libraries = Assert.NotNull(libraries, nameof(libraries));

        Options = options ?? ParseOptions.Default();

        RootNode = new LexyScriptNode(project);
        LineFilter = new DefaultLineFilter();
        Symbols = new Symbols.Symbols(RootNode);
    }

    public void AddFileIncluded(IFile file)
    {
        Assert.NotNull(file, nameof(file));

        var path = file.FullPath;

        includedFiles.Add(path);
    }

    public bool IsFileIncluded(IFile file)
    {
        Assert.NotNull(file, nameof(file));

        return includedFiles.Contains(file.FullPath);
    }

    public void SetFileLineFilter(IFile file)
    {
        Assert.NotNull(file, nameof(file));

        LineFilter = file.Name.EndsWith(LexySourceDocument.MarkdownExtension) ? new MarkdownLineFilter() : new DefaultLineFilter();
    }
}
