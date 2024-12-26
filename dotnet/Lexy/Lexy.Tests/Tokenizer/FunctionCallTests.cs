using Lexy.Poc.Core;
using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class FunctionCallTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestIntTypeLiteral()
        {
            ServiceProvider
                .TestLine(@"   LOOKUP(SimpleTable, Value, ""Result"")")
                .ValidateTokens()
                    .Count(8)
                    .StringLiteral(0, "LOOKUP")
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
}