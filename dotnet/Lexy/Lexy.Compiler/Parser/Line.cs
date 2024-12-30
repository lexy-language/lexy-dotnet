using System;

namespace Lexy.Compiler.Parser;

public class Line
{
    public int Index { get; }

    internal string Content { get; }
    private string TrimmedContent { get; }
    public SourceFile File { get; }

    public TokenList Tokens { get; private set; }

    public Line(int index, string line, SourceFile file)
    {
        Index = index;
        Content = line ?? throw new ArgumentNullException(nameof(line));
        File = file ?? throw new ArgumentNullException(nameof(file));
        TrimmedContent = line.Trim();
    }

    public int? Indent(IParserContext parserContext)
    {
        var spaces = 0;
        var tabs = 0;

        var index = 0;
        for (; index < Content.Length; index++)
        {
            var value = Content[index];
            if (value == ' ')
                spaces++;
            else if (value == '\t')
                tabs++;
            else
                break;
        }

        if (spaces > 0 && tabs > 0)
        {
            parserContext.Logger.Fail(LineReference(index),
                "Don't mix spaces and tabs for indentations. Use 2 spaces or tabs.");
            return null;
        }

        if (spaces % 2 != 0)
        {
            parserContext.Logger.Fail(LineReference(index),
                $"Wrong number of indent spaces {spaces}. Should be multiplication of 2. (line: {Index} line: {Content})");
            return null;
        }

        return tabs > 0 ? tabs : spaces / 2;
    }

    public override string ToString()
    {
        return $"{Index + 1}: {Content}";
    }

    public bool IsEmpty()
    {
        return Tokens.Length == 0;
    }

    public int? FirstCharacter()
    {
        for (var index = 0; index < Content.Length; index++)
            if (Content[index] != ' ' && Content[index] != '\\')
                return index;

        return 0;
    }

    public SourceReference TokenReference(int tokenIndex)
    {
        return new SourceReference(
            File,
            Index + 1,
            Tokens.CharacterPosition(tokenIndex) + 1);
    }

    public SourceReference LineEndReference()
    {
        return new SourceReference(File,
            Index + 1,
            Content.Length);
    }

    public SourceReference LineStartReference()
    {
        var lineStart = FirstCharacter();
        return new SourceReference(File,
            Index + 1,
            lineStart + 1);
    }

    public SourceReference LineReference(int characterIndex)
    {
        return new SourceReference(File ?? new SourceFile("runtime"),
            Index + 1,
            characterIndex + 1);
    }

    public void SetTokens(TokenList tokens)
    {
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }
}