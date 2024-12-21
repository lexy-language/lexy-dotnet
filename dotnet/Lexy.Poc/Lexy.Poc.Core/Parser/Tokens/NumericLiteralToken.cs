using System;
using System.Globalization;

namespace Lexy.Poc.Core.Parser
{
    public class NumberLiteralToken : ParsableToken, ILiteralToken
    {
        private bool hasDecimalSeparator;
        private decimal? numberValue;

        public decimal NumberValue
        {
            get
            {
                if (!numberValue.HasValue)
                {
                    throw new InvalidOperationException("NumberLiteralToken not finalized.");
                }
                return numberValue.Value;
            }
        }

        public override string Value => numberValue.HasValue
            ? numberValue.Value.ToString(CultureInfo.InvariantCulture)
            : base.Value;

        public NumberLiteralToken(char value) : base(value)
        {
        }

        public override ParseTokenResult Parse(char value, ParserContext parserContext)
        {
            if (char.IsDigit(value))
            {
                AppendValue(value);
                return ParseTokenResult.InProgress();
            }

            if (value == TokenValues.DecimalSeparator)
            {
                if (hasDecimalSeparator)
                {
                    return ParseTokenResult.Invalid("Only one decimal separator expected");
                }

                hasDecimalSeparator = true;
                AppendValue(value);
                return ParseTokenResult.InProgress();
            }

            return Finish();
        }

        public override ParseTokenResult Finalize(ParserContext parserContext)
        {
            return Finish();
        }

        private ParseTokenResult Finish()
        {
            numberValue = decimal.Parse(base.Value, CultureInfo.InvariantCulture);
            return ParseTokenResult.Finished(false);
        }
    }
}