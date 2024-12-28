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

    public ParserContext(ITokenizer tokenizer, IParserLogger logger, ISourceCodeDocument sourceCodeDocument)
    {
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SourceCode = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));

        RootNode = new SourceCodeNode();
    }

    public Line CurrentLine => SourceCode.CurrentLine;

    public RootNodeList Nodes => RootNode.RootNodes;
    public SourceCodeNode RootNode { get; }

    public ISourceCodeDocument SourceCode { get; }

    public IParserLogger Logger { get; }

    public void ProcessNode(IRootNode node)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));

        Logger.SetCurrentNode(node);
    }

    public bool ProcessLine()
    {
        var line = SourceCode.NextLine();
        Logger.Log(LineStartReference(), $"'{line.Content}'");

        var success = CurrentLine.Tokenize(tokenizer, this);
        var tokenNames = string.Join(" ", CurrentLine.Tokens.Select(token =>
            $"{token.GetType().Name}({token.Value})").ToArray());

        Logger.Log(LineStartReference(), "  Tokens: " + tokenNames);

        return success;
    }

    public TokenValidator ValidateTokens<T>()
    {
        Logger.Log(LineStartReference(), "  Parse: " + typeof(T).Name);
        return new TokenValidator(typeof(T).Name, this);
    }

    public TokenValidator ValidateTokens(string name)
    {
        Logger.Log(LineStartReference(), "  Parse: " + name);
        return new TokenValidator(name, this);
    }

    public SourceReference TokenReference(int tokenIndex)
    {
        return new SourceReference(
            SourceCode.File,
            SourceCode.CurrentLine?.Index + 1,
            SourceCode.CurrentLine?.Tokens.CharacterPosition(tokenIndex) + 1);
    }

    public SourceReference LineEndReference()
    {
        return new SourceReference(SourceCode.File,
            SourceCode.CurrentLine?.Index + 1,
            SourceCode.CurrentLine.Content.Length);
    }

    public SourceReference LineStartReference()
    {
        var lineStart = SourceCode.CurrentLine?.FirstCharacter();
        return new SourceReference(SourceCode.File,
            SourceCode.CurrentLine?.Index + 1,
            lineStart + 1);
    }

    public SourceReference LineReference(int characterIndex)
    {
        return new SourceReference(SourceCode.File ?? new SourceFile("runtime"),
            SourceCode.CurrentLine?.Index + 1,
            characterIndex + 1);
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