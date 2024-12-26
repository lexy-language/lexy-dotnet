using System;
using System.Linq;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Parser
{
    public class ParserContext : IParserContext
    {
        private readonly IParserLogger logger;
        private readonly ITokenizer tokenizer;
        private readonly ISourceCodeDocument sourceCodeDocument;

        public Line CurrentLine => sourceCodeDocument.CurrentLine;

        public Nodes Nodes { get; } = new Nodes();
        public ISourceCodeDocument SourceCode => sourceCodeDocument;
        public IParserLogger Logger => logger;

        public ParserContext(ITokenizer tokenizer, IParserLogger logger, ISourceCodeDocument sourceCodeDocument)
        {
            this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sourceCodeDocument = sourceCodeDocument ?? throw new ArgumentNullException(nameof(sourceCodeDocument));
        }

        public void ProcessNode(IRootNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            Nodes.AddIfNew(node);

            logger.SetCurrentNode(node);
        }

        public bool ProcessLine()
        {
            var line = sourceCodeDocument.NextLine();
            logger.Log(LineStartReference(), $"'{line.Content}'");

            var success = CurrentLine.Tokenize(tokenizer, this);
            var tokenNames = string.Join(" ", CurrentLine.Tokens.Select(token =>
                $"{token.GetType().Name}({token.Value})").ToArray());

            logger.Log(LineStartReference(), "  Tokens: " + tokenNames);

            return success;
        }

        public TokenValidator ValidateTokens<T>()
        {
            logger.Log(LineStartReference(), "  Parse: " + typeof(T).Name);
            return new TokenValidator(typeof(T).Name, this);
        }

        public TokenValidator ValidateTokens(string name)
        {
            logger.Log(LineStartReference(), "  Parse: " + name);
            return new TokenValidator(name, this);
        }

        public SourceReference TokenReference(int tokenIndex)
        {
            return new SourceReference(
                sourceCodeDocument.File,
                sourceCodeDocument.CurrentLine?.Index + 1,
                sourceCodeDocument.CurrentLine?.Tokens.CharacterPosition(tokenIndex) + 1);
        }

        public SourceReference TokenReference(Token token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            return new SourceReference(
                sourceCodeDocument.File,
                sourceCodeDocument.CurrentLine?.Index + 1,
                token.FirstCharacter.Position + 1);
        }

        public SourceReference LineEndReference()
        {
            return new SourceReference(sourceCodeDocument.File, sourceCodeDocument.CurrentLine?.Index + 1,
                sourceCodeDocument.CurrentLine.Content.Length);
        }

        public SourceReference LineStartReference()
        {
            var lineStart = sourceCodeDocument.CurrentLine?.FirstCharacter();
            return new SourceReference(sourceCodeDocument.File, sourceCodeDocument.CurrentLine?.Index + 1, lineStart);
        }

        public SourceReference DocumentReference()
        {
            return new SourceReference(sourceCodeDocument.File, 1, 1);
        }

        public SourceReference LineReference(int characterIndex)
        {
            return new SourceReference(sourceCodeDocument.File ?? new SourceFile("runtime"), sourceCodeDocument.CurrentLine?.Index + 1, characterIndex + 1);
        }
    }
}