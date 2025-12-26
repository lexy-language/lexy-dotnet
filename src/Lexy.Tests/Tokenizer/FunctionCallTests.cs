using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class FunctionCallTests : ScopedServicesTestFixture
{
    [Test]
    public void TestIntTypeLiteral()
    {
        ServiceProvider
            .Tokenize(@"   lookUp(SimpleTable, Value, ""Result"")")
            .Count(8)
            .StringLiteral(0, "lookUp")
            .Operator(1, OperatorType.OpenParentheses)
            .StringLiteral(2, "SimpleTable")
            .Operator(3, OperatorType.ArgumentSeparator)
            .StringLiteral(4, "Value")
            .Operator(5, OperatorType.ArgumentSeparator)
            .QuotedString(6, "Result")
            .Operator(7, OperatorType.CloseParentheses)
            .Assert();
    }
}