using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser.Tokens;

namespace Lexy.Poc.Core.Parser
{
    public class Tokenizer : ITokenizer
    {
        private readonly IDictionary<char, Func<char, ParsableToken>> knownTokens =
            new Dictionary<char, Func<char, ParsableToken>>
            {
                { TokenValues.CommentChar, value => new CommentToken(value) },
                { TokenValues.Quote, value => new QuotedLiteralToken(value) },
                { TokenValues.Assignment, value => new OperatorToken(value) },
                { TokenValues.Addition, value => new OperatorToken(value) },
                { TokenValues.Subtraction, value => new OperatorToken(value) },
                { TokenValues.Multiplication, value => new OperatorToken(value) },
                { TokenValues.Division, value => new OperatorToken(value) },
                { TokenValues.Modulus, value => new OperatorToken(value) },

                { TokenValues.OpenParentheses, value => new OperatorToken(value) },
                { TokenValues.CloseParentheses, value => new OperatorToken(value) },
                { TokenValues.OpenBrackets, value => new OperatorToken(value) },
                { TokenValues.CloseBrackets, value => new OperatorToken(value) },

                { TokenValues.GreaterThan, value => new OperatorToken(value) },
                { TokenValues.LessThan, value => new OperatorToken(value) },

                { TokenValues.NotEqualStart, value => new OperatorToken(value) },

                { TokenValues.And, value => new OperatorToken(value) },
                { TokenValues.Or, value => new OperatorToken(value) },
            };

        private readonly IDictionary<Func<char, bool>, Func<char, ParsableToken>> tokensValidators =
            new Dictionary<Func<char, bool>, Func<char, ParsableToken>>
            {
                { char.IsDigit, value => new NumberLiteralToken(value)},
                { char.IsLetter, value => new BuildLiteralToken(value)},
                { char.IsWhiteSpace, value => new WhitespaceToken(value)}
            };

        public TokenList Tokenize(Line line, ParserContext parserContext, out bool errors)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));

            errors = false;
            var tokens = new List<Token>();
            ParsableToken current = null;

            for (var index = 0; index < line.Content.Length; index++)
            {
                var value = line.Content[index];
                var valueProcessed = false;
                if (current != null)
                {
                    var result = current.Parse(value, parserContext);
                    if (result.Status == TokenStatus.InvalidToken)
                    {
                        parserContext.Logger.Fail($"Invalid token at {index}: {result.ValidationError}", parserContext.CurrentComponent);
                        errors = true;
                        break;
                    }

                    if (result.Status == TokenStatus.Finished)
                    {
                        tokens.Add(result.NewToken ?? current);
                        current = null;
                        valueProcessed = result.CharProcessed;
                    }
                    else if (result.Status == TokenStatus.InProgress && result.NewToken != null)
                    {
                        var parsableToken = result.NewToken as ParsableToken;
                        current = parsableToken ?? throw new InvalidOperationException(
                            "New token can only be a parsable token when in progress");
                    }
                }

                if (current == null && !valueProcessed)
                {
                    current = StartToken(value, index, parserContext);
                }
            }

            if (!errors && current != null)
            {
                var result = current.Finalize(parserContext);
                if (result.Status != TokenStatus.Finished)
                {
                    parserContext.Logger.Fail($"Invalid token at end of line. {result.ValidationError}", parserContext.CurrentComponent);
                    errors = true;
                }
                else
                {
                    tokens.Add(result.NewToken ?? current);
                }
            }

            return DiscardWhitespace(tokens);
        }

        private static TokenList DiscardWhitespace(List<Token> tokens)
        {
            return new TokenList(tokens.Where(token => !(token is WhitespaceToken)).ToArray());
        }

        private ParsableToken StartToken(char value, int index, ParserContext parserContext)
        {
            if (knownTokens.ContainsKey(value))
            {
                return knownTokens[value](value);
            }

            foreach (var validator in tokensValidators)
            {
                if (validator.Key(value))
                {
                    return validator.Value(value);
                }
            }

            parserContext.Logger.Fail($"Invalid character at {index} '{value}'", parserContext.CurrentComponent);
            return null;
        }
    }
}