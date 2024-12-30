using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public class ParserContext : IParserContext
{
    private readonly IList<string> includedFiles = new List<string>();

    private readonly ITokenizer tokenizer;

    public Line CurrentLine => SourceCode.CurrentLine;

    public RootNodeList Nodes => RootNode.RootNodes;
    public SourceCodeNode RootNode { get; }

    public ISourceCodeDocument SourceCode { get; }
    public IParserLogger Logger { get; }

    public ParserContext(ITokenizer tokenizer, IParserLogger logger, ISourceCodeDocument sourceCodeDocument)
    {
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SourceCode = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));

        RootNode = new SourceCodeNode();
    }


    public bool ProcessLine()
    {
        var line = SourceCode.NextLine();
        Logger.Log(line.LineStartReference(), $"'{line.Content}'");

        var tokens = tokenizer.Tokenize(line);
        if (!tokens.IsSuccess)
        {
            Logger.Fail(tokens.Reference, tokens.ErrorMessage);
            return false;
        }

        line.SetTokens(tokens.Result);

        var tokenNames = string.Join(" ", CurrentLine.Tokens.Select(token =>
            $"{token.GetType().Name}({token.Value})").ToArray());

        Logger.Log(line.LineStartReference(), "  Tokens: " + tokenNames);

        return tokens.IsSuccess;
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