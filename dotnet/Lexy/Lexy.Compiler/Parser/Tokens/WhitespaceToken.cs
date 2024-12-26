namespace Lexy.Poc.Core.Parser.Tokens
{
    internal class WhitespaceToken : ParsableToken
    {
        public WhitespaceToken(TokenCharacter character) : base(character)
        {
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext parserContext)
        {
            var value = character.Value;
            return !char.IsWhiteSpace(value)
                ? ParseTokenResult.Finished(false)
                : ParseTokenResult.InProgress();
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            return ParseTokenResult.Finished(true);
        }
    }
}