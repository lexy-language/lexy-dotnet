using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class MemberAccessTests : ScopedServicesTestFixture
{
    [Test]
    public void TestTableHeader()
    {
        ServiceProvider
            .Tokenize(@"    Source.Member")
            .ValidateTokens()
            .Count(1)
            .Type<MemberAccessLiteral>(0)
            .MemberAccess(0, "Source.Member")
            .Assert();
    }
}