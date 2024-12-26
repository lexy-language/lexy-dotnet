using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class StringLiteralsTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestQuotedLiteral()
        {
            ServiceProvider
                .TestLine(@"   ""This is a quoted literal""")
                .ValidateTokens()
                    .Count(1)
                    .QuotedString(0, "This is a quoted literal")
                .Assert();
        }

        [Test]
        public void TestStringLiteral()
        {
            ServiceProvider
                .TestLine(@"   ThisIsAStringLiteral")
                .ValidateTokens()
                    .Count(1)
                    .StringLiteral(0, "ThisIsAStringLiteral")
                .Assert();
        }
    }
}