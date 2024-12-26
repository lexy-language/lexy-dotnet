using Lexy.Poc.Core;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class ParametersTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestParameterDeclaration()
        {
            ServiceProvider
                .TestLine("  number Result")
                .ValidateTokens()
                    .Count(2)
                    .StringLiteral(0,"number")
                    .StringLiteral(1,"Result")
                .Assert();
        }
    }
}