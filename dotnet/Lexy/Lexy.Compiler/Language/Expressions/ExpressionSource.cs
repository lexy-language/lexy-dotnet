using System;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Language.Expressions
{
    public class ExpressionSource
    {
        public SourceFile File { get; }
        public Line Line { get; }
        public TokenList Tokens { get; }

        public ExpressionSource(SourceFile file, Line line, TokenList tokens)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Line = line ?? throw new ArgumentNullException(nameof(line));
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
}