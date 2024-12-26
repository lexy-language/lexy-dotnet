using System.Collections.Generic;
using System.Linq;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public class OperatorToken : ParsableToken
    {
        private class OperatorCombinations
        {
            public char FirstChar { get; }
            public char? SecondChar { get; }
            public OperatorType Type { get; }

            public OperatorCombinations(char firstChar, char? secondChar, OperatorType type)
            {
                FirstChar = firstChar;
                SecondChar = secondChar;
                Type = type;
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
        };

        private readonly IList<OperatorCombinations> operatorCombinations = new List<OperatorCombinations>()
        {
            new OperatorCombinations(TokenValues.Assignment, null, OperatorType.Assignment),
            new OperatorCombinations(TokenValues.Addition, null, OperatorType.Addition),
            new OperatorCombinations(TokenValues.Subtraction, null, OperatorType.Subtraction),
            new OperatorCombinations(TokenValues.Multiplication, null, OperatorType.Multiplication),
            new OperatorCombinations(TokenValues.Division, null, OperatorType.Division),
            new OperatorCombinations(TokenValues.Modulus, null, OperatorType.Modulus),
            new OperatorCombinations(TokenValues.OpenParentheses, null, OperatorType.OpenParentheses),
            new OperatorCombinations(TokenValues.CloseParentheses, null, OperatorType.CloseParentheses),
            new OperatorCombinations(TokenValues.OpenBrackets, null, OperatorType.OpenBrackets),
            new OperatorCombinations(TokenValues.CloseBrackets, null, OperatorType.CloseBrackets),
            new OperatorCombinations(TokenValues.GreaterThan, null, OperatorType.GreaterThan),
            new OperatorCombinations(TokenValues.LessThan, null, OperatorType.LessThan),
            new OperatorCombinations(TokenValues.ArgumentSeparator, null, OperatorType.ArgumentSeparator),
            new OperatorCombinations(TokenValues.GreaterThan, TokenValues.Assignment, OperatorType.GreaterThanOrEqual),
            new OperatorCombinations(TokenValues.LessThan, TokenValues.Assignment, OperatorType.LessThanOrEqual),
            new OperatorCombinations(TokenValues.Assignment, TokenValues.Assignment, OperatorType.Equals),
            new OperatorCombinations(TokenValues.NotEqualStart, TokenValues.Assignment, OperatorType.NotEqual),
            new OperatorCombinations(TokenValues.And, TokenValues.And, OperatorType.And),
            new OperatorCombinations(TokenValues.Or, TokenValues.Or, OperatorType.Or),
        };

        public OperatorType Type { get; private set; } = OperatorType.NotSet;

        public OperatorToken(TokenCharacter character) : base(character)
        {
            var operatorValue = character.Value;
            foreach (var combination in operatorCombinations)
            {
                if (!combination.SecondChar.HasValue && combination.FirstChar == operatorValue)
                {
                    Type = combination.Type;
                }
            }
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext context)
        {
            var value = character.Value;
            if (Value.Length == 1)
            {
                foreach (var combination in operatorCombinations)
                {
                    if (combination.SecondChar.HasValue
                        && combination.SecondChar.Value == value
                        && combination.FirstChar == Value[0])
                    {
                        Type = combination.Type;
                        return ParseTokenResult.Finished(true);
                    }
                }
            }

            if (char.IsLetterOrDigit(value)
             || TerminatorValues.Contains(value))
            {
                if (Value.Length == 1 && Value[0] == TokenValues.TableSeparator)
                {
                    return ParseTokenResult.Finished(false, new TableSeparatorToken(FirstCharacter));
                }
                return ParseTokenResult.Finished(false);
            }

            return ParseTokenResult.Invalid($"Invalid token: {value}");
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            if (Value == TokenValues.TableSeparator.ToString())
            {
                return ParseTokenResult.Finished(false, new TableSeparatorToken(FirstCharacter));
            }

            return ParseTokenResult.Finished(false);
        }
    }
}