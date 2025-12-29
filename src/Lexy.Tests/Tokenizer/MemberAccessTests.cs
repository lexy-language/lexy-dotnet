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
            .Count(1)
            .Type<MemberAccessLiteralToken>(0)
            .MemberAccess(0, "Source.Member")
            .Assert();
    }
}