using Lexy.Poc.Core;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class TypeLiteralsTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestIntTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   int Value")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0, "int")
                    .StringLiteral(1, "Value")
                .Assert();
        }

        [Test]
        public void TestNumberTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   number Value")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0, "number")
                    .StringLiteral(1, "Value")
                .Assert();
        }

        [Test]
        public void TestStringTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   string Value")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0, "string")
                    .StringLiteral(1, "Value")
                .Assert();
        }

        [Test]
        public void TestDateTimeTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   date Value")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0, "date")
                    .StringLiteral(1, "Value")
                .Assert();
        }

        [Test]
        public void TestBooleanTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   boolean Value")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0, "boolean")
                    .StringLiteral(1, "Value")
                .Assert();
        }
    }
}