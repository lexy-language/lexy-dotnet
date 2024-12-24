using System;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser.Tokens
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
                return ParseTokenResult.Finished(true, CheckForKeywords());
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

            return ParseTokenResult.Finished(true, CheckForKeywords());
        }

        private Token CheckForKeywords()
        {
            var keyword = Value;
            if (Keywords.Contains(keyword))
            {
                return new KeywordToken(keyword, FirstCharacter);
            }

            return this;
        }

        public override string ToString() => Value;

        public VariableType DeriveType(IValidationContext context) => PrimitiveType.String;
    }
}