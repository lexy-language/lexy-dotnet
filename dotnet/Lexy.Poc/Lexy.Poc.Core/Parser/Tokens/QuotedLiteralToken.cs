using System;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public class QuotedLiteralToken : ParsableToken, ILiteralToken
    {
        private bool quoteClosed;

        public QuotedLiteralToken(char value)
        {
            if (value != TokenValues.Quote)
            {
                throw new InvalidOperationException("QuotedLiteralToken should start with a quote");
            }
        }

        public override ParseTokenResult Parse(char value, ParserContext parserContext)
        {
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

        public override ParseTokenResult Finalize(ParserContext parserContext)
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
                return new KeywordToken(keyword);
            }

            return this;
        }
    }
}