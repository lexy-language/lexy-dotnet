using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser.Tokens
{
    public class DateTimeLiteral : ParsableToken, ILiteralToken
    {
        //format d"0123/56/89 12:45:78"
        private static readonly int[] DigitIndexes = { 0, 1, 2, 3, 5, 6, 8, 9, 11, 12, 14, 15, 17, 18 };
        private static readonly int[] SlashIndexes = { 4, 7 };
        private static readonly int[] SpaceIndexes = { 10 };
        private static readonly int[] ColonIndexes = { 13, 16 };
        private static readonly int[] ValidLengths = { 19 };

        private const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        private int index;
        private readonly IList<Func<char, ParseTokenResult>> validators;

        public DateTime DateTimeValue { get; set; }

        public object TypedValue => DateTimeValue;

        public DateTimeLiteral(TokenCharacter character) : base(null, character)
        {
            validators = new List<Func<char, ParseTokenResult>>
            {
                value => Validate(value, char.IsDigit(value), DigitIndexes),
                value => Validate(value, TokenValues.Slash, SlashIndexes),
                value => Validate(value, TokenValues.Colon, ColonIndexes),
                value => Validate(value, TokenValues.Space, SpaceIndexes)
            };
        }

        public override ParseTokenResult Parse(TokenCharacter character, IParserContext context)
        {
            var value = character.Value;
            if (value == TokenValues.Quote)
            {
                if (!ValidLengths.Contains(Value.Length))
                {
                    return ParseTokenResult.Invalid(
                        $"Invalid date time format length '{Value.Length}'. Expected: '{ValidLengths.FormatLine(",")}'");
                }

                ParseValue();
                return ParseTokenResult.Finished(true);
            }

            foreach (var validator in validators)
            {
                var result = validator(value);
                if (result != null)
                {
                    AppendValue(value);
                    index++;
                    return result;
                }
            }

            return ParseTokenResult.Invalid($@"Unexpected character: '{value}'. Format: d""2024/12/18 14:17:30""");
        }

        private void ParseValue()
        {
            DateTimeValue = DateTime.ParseExact(Value, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        private ParseTokenResult Validate(char value, char match, int[] indexes) => Validate(value, value == match, indexes);

        private ParseTokenResult Validate(char value, bool match, int[] indexes)
        {
            if (!match) return null;

            if (!indexes.Contains(index))
                return ParseTokenResult.Invalid($@"Unexpected character: '{value}'. Format: d""2024/12/18 14:17:30""");

            return ParseTokenResult.InProgress();
        }

        public override ParseTokenResult Finalize(IParserContext parserContext)
        {
            return ParseTokenResult.Invalid(
                $@"Unexpected end of line. Closing quote expected. Format: d""2024/12/18 14:17:30""");
        }

        public override string ToString() => DateTimeValue.ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        public VariableType DeriveType(IValidationContext context) => PrimitiveType.Date;
    }
}