using System.Collections.Generic;
using System.Linq;

namespace Lexy.Compiler.Parser.Tokens;

public class OperatorToken : ParsableToken
{
    private enum CombinationMatch
    {
        Invalid,
        Incomplete,
        Complete,
        CompleteNotProcessed
    }

    private class OperatorCombinations
    {
        private readonly char firstChar;
        private readonly char? secondChar;
        private readonly char? thirdChar;
        private readonly int chars;

        public OperatorType Type { get; }

        public OperatorCombinations(OperatorType type, char firstChar, char? secondChar = null, char? thirdChar = null)
        {
            Type = type;
            this.firstChar = firstChar;
            this.secondChar = secondChar;
            this.thirdChar = thirdChar;
            chars = thirdChar.HasValue ? 3 : secondChar.HasValue ? 2 : 1;
        }

        public CombinationMatch Matches(char firstCharacter, char? secondCharacter, char? thirdCharacter)
        {
            if (firstCharacter != firstChar) return CombinationMatch.Invalid;

            if (!secondCharacter.HasValue)
            {
                return chars == 1 ? CombinationMatch.Complete : CombinationMatch.Invalid;
            }

            if (!thirdCharacter.HasValue)
            {
                return chars switch
                {
                    3 => secondCharacter == secondChar ? CombinationMatch.Incomplete : CombinationMatch.Invalid,
                    2 => secondCharacter == secondChar ? CombinationMatch.Complete : CombinationMatch.Invalid,
                    1 => CombinationMatch.CompleteNotProcessed,
                };
            }

            return thirdCharacter == thirdChar && secondCharacter == secondChar ? CombinationMatch.Complete : CombinationMatch.Invalid;
        }
    }

    private static readonly char[] TerminatorValues =
    {
        TokenValues.Space,
        TokenValues.ArgumentSeparator,
        TokenValues.Subtraction,
        TokenValues.OpenParentheses,
        TokenValues.OpenBrackets,
        TokenValues.CloseParentheses,
        TokenValues.CloseBrackets,
        TokenValues.Quote
    };

    private static readonly IList<OperatorCombinations> operatorCombinations = new List<OperatorCombinations>
    {
        new(OperatorType.GreaterThanOrEqual, TokenValues.GreaterThan, TokenValues.Assignment),
        new(OperatorType.LessThanOrEqual, TokenValues.LessThan, TokenValues.Assignment),
        new(OperatorType.Equals, TokenValues.Assignment, TokenValues.Assignment),
        new(OperatorType.NotEqual, TokenValues.NotEqualStart, TokenValues.Assignment),
        new(OperatorType.And, TokenValues.And, TokenValues.And),
        new(OperatorType.Or, TokenValues.Or, TokenValues.Or),

        new(OperatorType.Assignment, TokenValues.Assignment),
        new(OperatorType.Addition, TokenValues.Addition),
        new(OperatorType.Subtraction, TokenValues.Subtraction),
        new(OperatorType.Multiplication, TokenValues.Multiplication),
        new(OperatorType.Division, TokenValues.DivisionOrComment),
        new(OperatorType.Modulus, TokenValues.Modulus),
        new(OperatorType.OpenParentheses, TokenValues.OpenParentheses),
        new(OperatorType.CloseParentheses, TokenValues.CloseParentheses),
        new(OperatorType.OpenBrackets, TokenValues.OpenBrackets),
        new(OperatorType.CloseBrackets, TokenValues.CloseBrackets),
        new(OperatorType.GreaterThan, TokenValues.GreaterThan),
        new(OperatorType.LessThan, TokenValues.LessThan),
        new(OperatorType.ArgumentSeparator, TokenValues.ArgumentSeparator),

        new(OperatorType.Spread, TokenValues.Spread, TokenValues.Spread, TokenValues.Spread)
    };

    public OperatorType Type { get; private set; } = OperatorType.NotSet;

    public OperatorToken(TokenCharacter character, OperatorType operatorType) : base(character)
    {
        Type = operatorType;
    }

    public OperatorToken(TokenCharacter character) : base(character)
    {
    }

    public override ParseTokenResult Parse(TokenCharacter character)
    {
        var nextCharacter = character.Value;
        var firstCharacter = Value[0];
        var secondCharacter = Value.Length == 2 ? Value[1] : nextCharacter;
        var thirdCharacter = Value.Length == 2 ? nextCharacter : (char?) null;

        foreach (var combination in operatorCombinations)
        {
            var matches = combination.Matches(firstCharacter, secondCharacter, thirdCharacter);

            if (matches == CombinationMatch.Invalid) continue;

            return ParseToken(matches, combination, nextCharacter);
        }

        if (char.IsLetterOrDigit(nextCharacter) || TerminatorValues.Contains(nextCharacter))
        {
            if (Value.Length == 1 && Value[0] == TokenValues.TableSeparator)
            {
                return ParseTokenResult.Finished(false, new TableSeparatorToken(FirstCharacter));
            }
            return ParseTokenResult.Finished(false);
        }

        return ParseTokenResult.Invalid($"Invalid token at {character.Position}: '{nextCharacter}'");
    }

    private ParseTokenResult ParseToken(CombinationMatch matches, OperatorCombinations combination, char nextCharacter)
    {
        if (matches is CombinationMatch.Incomplete)
        {
            AppendValue(nextCharacter);
            return ParseTokenResult.InProgress();
        }

        Type = combination.Type;

        if (matches is CombinationMatch.Complete)
        {
            AppendValue(nextCharacter);
        }

        return ParseTokenResult.Finished(matches == CombinationMatch.Complete);
    }

    public override ParseTokenResult EndOfLine()
    {
        if (Value == TokenValues.TableSeparator.ToString())
        {
            return ParseTokenResult.Finished(false, new TableSeparatorToken(FirstCharacter));
        }

        var firstCharacter = Value[0];
        char? secondCharacter = Value.Length > 1 ? Value[1] : null;
        char? thirdCharacter = Value.Length > 2 ? Value[2] : null;

        foreach (var combination in operatorCombinations)
        {
            var matches = combination.Matches(firstCharacter, secondCharacter, thirdCharacter);

            if (matches == CombinationMatch.Complete
             || matches == CombinationMatch.CompleteNotProcessed)
            {
                Type = combination.Type;
                return ParseTokenResult.Finished(false);
            }
        }

        return ParseTokenResult.Invalid($"Incomplete token: '{Value}'");
    }
}