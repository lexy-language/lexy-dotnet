using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class MemberAccessTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestTableHeader()
        {
            ServiceProvider
                .TestLine(@"    Source.Member")
                .ValidateTokens()
                    .Count(1)
                    .Type<MemberAccessLiteral>(0)
                    .MemberAccess(0, "Source.Member")
                .Assert();
        }
    }
}