using System.Text;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public abstract class ParsableToken : Token
    {
        private readonly StringBuilder valueBuilder;

        public override string Value => valueBuilder.ToString();

        protected ParsableToken(TokenCharacter character) : base(character)
        {
            valueBuilder = new StringBuilder(character.Value.ToString());
        }

        protected ParsableToken(string value, TokenCharacter position) : base(position)
        {
            valueBuilder = new StringBuilder(value);
        }

        protected void AppendValue(char value)
        {
            valueBuilder.Append(value);
        }

        public abstract ParseTokenResult Parse(TokenCharacter character, IParserContext context);

        public abstract ParseTokenResult Finalize(IParserContext context);
    }
}