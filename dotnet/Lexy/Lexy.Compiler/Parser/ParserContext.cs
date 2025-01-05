using System;
using System.Collections.Generic;
using System.IO;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserContext : IParserContext
{
    private readonly IList<string> includedFiles = new List<string>();

    public RootNodeList Nodes => RootNode.RootNodes;

    public SourceCodeNode RootNode { get; }
    public IParserLogger Logger { get; }

    public ParserContext(IParserLogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RootNode = new SourceCodeNode();
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

    private static string NormalizePath(string fileName)
    {
        return Path.GetFullPath(fileName)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}