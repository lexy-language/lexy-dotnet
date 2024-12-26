namespace Lexy.Poc.Core.Parser.Tokens
{
    public class TableSeparatorToken : ParsableToken
    {
        public TableSeparatorToken(TokenCharacter character) : base(character)
        {
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext context)
        {
            return ParseTokenResult.Finished(true);
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            return ParseTokenResult.Finished(true);
        }
    }
}