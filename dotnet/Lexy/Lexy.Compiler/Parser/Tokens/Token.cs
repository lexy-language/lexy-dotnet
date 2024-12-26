using System;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public abstract class Token : IToken
    {
        public TokenCharacter FirstCharacter { get; }

        public abstract string Value { get; }

        protected Token(TokenCharacter firstCharacter)
        {
            FirstCharacter = firstCharacter ?? throw new ArgumentNullException(nameof(firstCharacter));
        }
    }
}