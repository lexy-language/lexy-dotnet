
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Parser
{
    public interface ITokenizer
    {
        TokenList Tokenize(Line line, ParserContext parserContext, out bool errors);
    }

    public class TokenList : IEnumerable<Token>
    {
        private readonly Token[] values;

        public Token this[int index] => values[index];

        public int Length => values.Length;

        public TokenList(Token[] values)
        {
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool IsComment()
        {
            return values.Length == 1 && values[0] is CommentToken;
        }
        public string TokenValue(int index)
        {
            return index >= 0 && index <= values.Length - 1 ? values[index].Value : null;
        }

        public TokenList TokensFrom(int index)
        {
            return new TokenList(values[index..]);
        }

        public string TokenValuesFrom(int startIndex)
        {
            var valueBuilder = new StringBuilder();
            for (int index = startIndex; index < values.Length; index++)
            {
                valueBuilder.Append(values[index].Value);
            }
            return valueBuilder.ToString();
        }

        public bool IsTokenType<T>(int index)
        {
            return index >= 0 && index <= values.Length - 1 && values[index].GetType() == typeof(T);
        }

        public Type TokenAsType<T>(int index)
        {
            return index >= 0 && index <= values.Length - 1 ? values[index].GetType() : null;
        }

        public T Token<T>(int index) where T : Token
        {
            return (T) values[index];
        }

        public ILiteralToken LiteralToken(int index)
        {
            return index >= 0 && index <= values.Length - 1 ? values[index] as ILiteralToken : null;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return ((IEnumerable<Token>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}