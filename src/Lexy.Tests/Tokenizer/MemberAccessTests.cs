using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class MemberAccessTests : ScopedServicesTestFixture
{
    [Test]
    public void Complete()
    {
        ServiceProvider
            .Tokenize(@"    Source.Member")
            .Count(1)
            .Type<MemberAccessToken>(0)
            .MemberAccess(0, "Source.Member")
            .Assert();
    }

    [Test]
    public void Incomplete()
    {
        ServiceProvider
            .Tokenize(@"    Source.")
            .Count(1)
            .Type<IncompleteMemberAccessToken>(0)
            .IncompleteMemberAccess(0, "Source.")
            .Assert();
    }
}
