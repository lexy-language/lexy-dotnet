using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public class Line
{
    public int Index { get; }

    internal string Content { get; }

    public IFile File { get; }

    public TokenList Tokens { get; private set; }
    public Position EndPosition { get; }

    public Line(int index, string line, IFile file)
    {
        Index = index;
        Content = Assert.NotNull(line, nameof(line));
        File = Assert.NotNull(file, nameof(file));
        EndPosition = new Position(index + 1, line.Length);
    }

    public int? Indent(IParserLogger logger)
    {
        var spaces = 0;
        var tabs = 0;
        var index = 0;

        while (index < Content.Length)
        {
            var value = Content[index];
            if (value == ' ')
            {
                spaces++;
            }
            else if (value == '\t')
            {
                tabs++;
            }
            else
            {
                break;
            }
            index++;
        }

        if (spaces > 0 && tabs > 0)
        {
            logger.Fail(LineReference(index),
                "Don't mix spaces and tabs for indentations. Use 2 spaces or tabs.");
            return null;
        }

        if (spaces % 2 != 0)
        {
            logger.Fail(LineReference(index),
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


    public SourceReference LineReference(int characterIndex)
    {
        return new SourceReference(File, Index + 1, characterIndex + 1, characterIndex + 1);
    }

    public SourceReference LineEndReference()
    {
        if (Tokens == null || Tokens.Length == 0)
        {
            return new SourceReference(File, Index + 1, 1, Content.Length + 1);
        }

        var columnEnd = Tokens[^1].EndColumn;
        return new SourceReference(File, Index + 1, columnEnd - 1, columnEnd);
    }

    public TokenizeResult Tokenize(ITokenizer tokenizer)
    {
        var tokenizeResult = tokenizer.Tokenize(this);
        if (tokenizeResult.IsSuccess)
        {
            Tokens = tokenizeResult.Result;
        }
        return tokenizeResult;
    }

    public TokenCharacter Character(int index)
    {
        var value = Content[index];
        return new TokenCharacter(value, index);
    }
}
