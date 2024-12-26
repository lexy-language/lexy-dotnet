using System;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser.Tokens
{
    public class QuotedLiteralToken : ParsableToken, ILiteralToken
    {
        private bool quoteClosed;

        public QuotedLiteralToken(TokenCharacter character) : base(null, character)
        {
            var value = character.Value;
            if (value != TokenValues.Quote)
            {
                throw new InvalidOperationException("QuotedLiteralToken should start with a quote");
            }
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext parserContext)
        {
            var value = character.Value;
            if (quoteClosed)
            {
                throw new InvalidOperationException("No characters allowed after closing quote.");
            }

            if (value == TokenValues.Quote)
            {
                quoteClosed = true;
                return ParseTokenResult.Finished(true, this);
            }

            AppendValue(value);
            return ParseTokenResult.InProgress();
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            if (!quoteClosed)
            {
                return ParseTokenResult.Invalid("Closing quote expected.");
            }

            return ParseTokenResult.Finished(true, this);
        }

        public override string ToString() => Value;

        public VariableType DeriveType(IValidationContext context) => PrimitiveType.String;
    }
}