using System;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class ExpressionSource
{
    public SourceFile File { get; }
    public Line Line { get; }
    public TokenList Tokens { get; }

    public ExpressionSource(Line line, TokenList tokens)
    {
        Line = line ?? throw new ArgumentNullException(nameof(line));
        File = line.File ?? throw new InvalidOperationException($"{nameof(line.File)} should not be null.");
        if (tokens?.Length == 0) throw new InvalidOperationException("No tokens.");
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
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