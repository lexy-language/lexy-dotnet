using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class ExpressionSource
{
    public IFile File { get; }
    public Line Line { get; }
    public TokenList Tokens { get; }

    public ExpressionSource(Line line, TokenList tokens)
    {
        Line = Assert.NotNull(line, nameof(line));
        File = Assert.NotNull(line.File, nameof(line.File));
        Tokens = Assert.NotNull(tokens, nameof(tokens));
    }

    public SourceReference CreateReference()
    {
        var token = Tokens[0];
        var tokenEnd = Tokens[^1];

        return new SourceReference(
            File,
            Line.Index + 1,
            token.FirstCharacter.Position + 1,
            tokenEnd.EndColumn + 1);
    }
}
