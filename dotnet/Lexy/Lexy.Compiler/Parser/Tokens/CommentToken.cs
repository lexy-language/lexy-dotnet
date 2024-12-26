
namespace Lexy.Poc.Core.Parser.Tokens
{
    public class CommentToken : ParsableToken
    {
        public CommentToken(TokenCharacter character) : base(character)
        {
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext parserContext)
        {
            AppendValue(character.Value);

            return ParseTokenResult.InProgress();
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            return ParseTokenResult.Finished(true);
        }
    }
}