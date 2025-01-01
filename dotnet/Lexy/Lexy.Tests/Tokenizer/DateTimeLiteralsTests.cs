using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class DateTimeLiteralsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestQuotedLiteral()
    {
        ServiceProvider
            .Tokenize(@"   OutDateTime = d""2024-12-16T13:26:55""")
            .Count(3)
            .DateTime(2, 2024, 12, 16, 13, 26, 55)
            .Assert();
    }
}