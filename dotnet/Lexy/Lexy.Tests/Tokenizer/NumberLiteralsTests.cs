using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class NumberLiteralsTests : ScopedServicesTestFixture
    {
        [Test]
        public void IntLiteral()
        {
            ServiceProvider
                .TestLine(@"   0")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 0)
                .Assert();
        }

        [Test]
        public void Int3CharLiteral()
        {
            ServiceProvider
                .TestLine(@"   456")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 456)
                .Assert();
        }


        [Test]
        public void NegativeIntLiteral()
        {
            ServiceProvider
                .TestLine(@"   -456")
                .ValidateTokens()
                    .Count(2)
                    .Operator(0, OperatorType.Subtraction)
                    .NumberLiteral(1, 456)
                .Assert();
        }

        [Test]
        public void DecimalLiteral()
        {
            ServiceProvider
                .TestLine(@"   456.78")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 456.78m)
                .Assert();
        }

        [Test]
        public void NegativeDecimalLiteral()
        {
            ServiceProvider
                .TestLine(@"   -456.78")
                .ValidateTokens()
                    .Count(2)
                    .Operator(0, OperatorType.Subtraction)
                    .NumberLiteral(1, 456.78m)
                .Assert();
        }

        [Test]
        public void InvalidDecimalSubtract()
        {
            ServiceProvider
                .TestLine(@"   456-78")
                .ValidateTokens()
                    .Count(3)
                    .NumberLiteral(0, 456)
                    .Operator(1, OperatorType.Subtraction)
                    .NumberLiteral(2, 78m)
                .Assert();
        }

        [Test]
        public void InvalidDecimalLiteral()
        {
            ServiceProvider
                .TestLine(@"   456d78", false)
                .ValidateError("ERROR - Invalid number token character: 'd'");
        }

        [Test]
        public void InvalidDecimalOpenParLiteral()
        {
            ServiceProvider
                .TestLine(@"   456(78", false)
                .ValidateError("ERROR - Invalid number token character: '('");
        }
    }
}