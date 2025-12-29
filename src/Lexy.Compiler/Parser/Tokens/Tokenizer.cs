using System;
using System.Collections.Generic;
using System.Linq;

namespace Lexy.Compiler.Parser.Tokens;

public class Tokenizer : ITokenizer
{
    private readonly IDictionary<char, Func<TokenCharacter, ParsableToken>> knownTokens =
        new Dictionary<char, Func<TokenCharacter, ParsableToken>>
        {
            { TokenValues.Quote, value => new QuotedLiteralToken(value) },

            { TokenValues.Assignment, value => new OperatorToken(value) },
            { TokenValues.Addition, value => new OperatorToken(value) },
            { TokenValues.Subtraction, value => new OperatorToken(value) },
            { TokenValues.Multiplication, value => new OperatorToken(value) },
            { TokenValues.DivisionOrComment, value => new BuildCommentOrDivisionToken(value) },
            { TokenValues.Modulus, value => new OperatorToken(value) },
            { TokenValues.ArgumentSeparator, value => new OperatorToken(value) },

            { TokenValues.OpenParentheses, value => new OperatorToken(value) },
            { TokenValues.CloseParentheses, value => new OperatorToken(value) },
            { TokenValues.OpenBrackets, value => new OperatorToken(value) },
            { TokenValues.CloseBrackets, value => new OperatorToken(value) },

            { TokenValues.GreaterThan, value => new OperatorToken(value) },
            { TokenValues.LessThan, value => new OperatorToken(value) },

            { TokenValues.NotEqualStart, value => new OperatorToken(value) },

            { TokenValues.And, value => new OperatorToken(value) },
            { TokenValues.Or, value => new OperatorToken(value) }
        };

    private readonly IDictionary<Func<char, bool>, Func<TokenCharacter, ParsableToken>> tokensValidators =
        new Dictionary<Func<char, bool>, Func<TokenCharacter, ParsableToken>>
        {
            { char.IsDigit, value => new NumberLiteralToken(value) },
            { char.IsLetter, value => new BuildLiteralToken(value) },
            { char.IsWhiteSpace, value => new WhitespaceToken(value) }
        };

    public TokenizeResult Tokenize(Line line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line));

        var tokens = new List<Token>();
        ParsableToken current = null;

        for (var index = 0; index < line.Content.Length; index++)
        {
            var tokenCharacter = line.Character(index);
            var valueProcessed = false;
            if (current != null)
            {
                var result = current.Parse(tokenCharacter);
                switch (result.Status)
                {
                    case TokenStatus.InvalidToken:
                    {
                        return TokenizeResult.Failed(line.LineReference(index), result.ValidationError);
                    }
                    case TokenStatus.Finished:
                    {
                        tokens.Add(result.NewToken ?? current);
                        current = null;
                        valueProcessed = result.CharProcessed;
                        break;
                    }
                    case TokenStatus.InProgress when result.NewToken != null:
                    {
                        var parsableToken = result.NewToken as ParsableToken;
                        current = parsableToken ?? throw new InvalidOperationException(
                            "New token can only be a parsable token when in progress");
                        break;
                    }
                }
            }

            if (current == null && !valueProcessed)
            {
                var parsableTokenResult = StartToken(tokenCharacter, index, line);
                if (!parsableTokenResult.IsSuccess)
                {
                    return TokenizeResult.Failed(parsableTokenResult.Reference, parsableTokenResult.ErrorMessage);
                }
                current = parsableTokenResult.Result;
            }
        }

        if (current != null)
        {
            var result = current.Finalize();
            if (result.Status != TokenStatus.Finished)
            {
                return TokenizeResult.Failed(line.LineEndReference(), $"Invalid token at end of line. {result.ValidationError}");
            }

            tokens.Add(result.NewToken ?? current);
        }

        return TokenizeResult.Success(DiscardWhitespaceAndComments(tokens));
    }

    private static TokenList DiscardWhitespaceAndComments(List<Token> tokens)
    {
        return new TokenList(tokens.Where(NotWhitespaceOrComment).ToArray());
    }

    private static bool NotWhitespaceOrComment(Token token) => token is not CommentToken && token is not WhitespaceToken;

    private ParsableTokenResult StartToken(TokenCharacter character, int index, Line line)
    {
        var value = character.Value;
        if (knownTokens.ContainsKey(value)) return ParsableTokenResult.Success(knownTokens[value](character));

        foreach (var validator in tokensValidators)
        {
            if (validator.Key(value))
            {
                return ParsableTokenResult.Success(validator.Value(character));
            }
        }

        return ParsableTokenResult.Failed(line.LineReference(index), $"Invalid character at {index} '{value}'") ;
    }
}