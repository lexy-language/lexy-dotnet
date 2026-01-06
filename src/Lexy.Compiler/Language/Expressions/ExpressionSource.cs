using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class ExpressionSource
{
    public SourceFile File { get; }
    public Line Line { get; }
    public TokenList Tokens { get; }

    public ExpressionSource(Line line, TokenList tokens)
    {
        Line = Assert.NotNull(line, nameof(line));
        File = Assert.NotNull(line.File, nameof(line));
        Tokens = Assert.NotNull(tokens, nameof(tokens));
    }

    public SourceReference CreateReference(int tokenIndex = 0)
    {
        var token = Tokens[tokenIndex];

        return new SourceReference(
            File,
            Line.Index + 1,
            token.FirstCharacter.Position + 1);
    }
}