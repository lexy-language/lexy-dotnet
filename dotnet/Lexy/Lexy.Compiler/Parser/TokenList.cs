using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Parser
{
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
            CheckValidTokenIndex(index);

            return new TokenList(values[index..]);
        }

        public TokenList TokensFromStart(int count)
        {
            return new TokenList(values[..count]);
        }

        public TokenList TokensRange(int start, int last)
        {
            var length = last + 1 - start;
            var range = new Token[length];

            Array.Copy(values, start, range, 0, length);

            return new TokenList(range);
        }

        public bool IsTokenType<T>(int index)
        {
            return index >= 0 && index <= values.Length - 1 && values[index].GetType() == typeof(T);
        }

        public T Token<T>(int index) where T : Token
        {
            CheckValidTokenIndex(index);

            return (T) values[index];
        }

        public ILiteralToken LiteralToken(int index)
        {
            CheckValidTokenIndex(index);

            return index >= 0 && index <= values.Length - 1 ? values[index] as ILiteralToken : null;
        }

        public bool IsLiteralToken(int index)
        {
            return index >= 0 && index <= values.Length - 1 && values[index] is ILiteralToken;
        }

        public bool IsKeyword(int index, string keyword)
        {
            return index >= 0
                && index <= values.Length - 1
                && (values[index] as KeywordToken)?.Value == keyword;
        }

        public bool OperatorToken(int index, OperatorType type)
        {
            return index >= 0
                && index <= values.Length - 1
                && values[index] is OperatorToken operatorToken
                && operatorToken.Type == type;
        }

        public OperatorToken OperatorToken(int index)
        {
            return index >= 0
                   && index <= values.Length - 1
                   && values[index] is OperatorToken operatorToken ? operatorToken : null;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return ((IEnumerable<Token>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var value in values)
            {
                builder.Append($"{value.GetType().Name}('{value.Value}') ");
            }
            return builder.ToString();
        }

        private void CheckValidTokenIndex(int index)
        {
            if (index < 0 || index >= values.Length)
            {
                throw new InvalidOperationException($"Invalid token index {index} (length: {values.Length})");
            }
        }

        public int? CharacterPosition(int tokenIndex)
        {
            if (tokenIndex < 0 || tokenIndex >= values.Length)
            {
                return null;
            }

            return values[tokenIndex].FirstCharacter.Position;
        }
    }
}