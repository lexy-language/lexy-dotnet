using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class ParametersTests : ScopedServicesTestFixture
{
    [Test]
    public void TestParameterDeclaration()
    {
        ServiceProvider
            .Tokenize("  number Result")
            .Count(2)
            .StringLiteral(0, "number")
            .StringLiteral(1, "Result")
            .Assert();
    }
}