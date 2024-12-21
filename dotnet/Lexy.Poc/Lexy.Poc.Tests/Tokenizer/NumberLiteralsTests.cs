using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class NumberLiteralsTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestInt0Literal()
        {
            ServiceProvider
                .TestLine(@"   0")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 0)
                .Assert();
        }

        [Test]
        public void TestIntLiteral()
        {
            ServiceProvider
                .TestLine(@"   456")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 456)
                .Assert();
        }

        [Test]
        public void TestDecimalLiteral()
        {
            ServiceProvider
                .TestLine(@"   456.78")
                .ValidateTokens()
                    .Count(1)
                    .NumberLiteral(0, 456.78m)
                .Assert();
        }
    }
}