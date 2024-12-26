using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class BooleanLiteralsTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestBooleanTrueLiteral()
        {
            ServiceProvider
                .TestLine(@"   true")
                .ValidateTokens()
                    .Count(1)
                    .Boolean(0, true)
                .Assert();
        }

        [Test]
        public void TestBooleanFalseLiteral()
        {
            ServiceProvider
                .TestLine(@"   false")
                .ValidateTokens()
                    .Count(1)
                    .Boolean(0, false)
                .Assert();
        }

    }
}