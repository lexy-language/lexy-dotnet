using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Poc.Tokenizer
{
    public class KeywordTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestFunctionKeyword()
        {
            ServiceProvider
                .TestLine("Function: TestSimpleReturn")
                .ValidateTokens()
                    .Count(2)
                    .Keyword(0, "Function:")
                    .StringLiteral(1, "TestSimpleReturn")
                .Assert();
        }

        [Test]
        public void TestResultKeyword()
        {
            ServiceProvider
                .TestLine("  Results")
                .ValidateTokens()
                    .Count(1)
                    .Keyword(0, "Results")
                .Assert();
        }

        [Test]
        public void TestExpectErrorKeywordWithQuotedLiteral()
        {
            ServiceProvider
                .TestLine(@"  ExpectError ""Invalid token 'Paraeters'""")
                .ValidateTokens()
                    .Count(2)
                    .Keyword(0, "ExpectError")
                    .QuotedString(1, "Invalid token 'Paraeters'")
                .Assert();
        }

        [Test]
        public void TestExpectErrorKeywordWithQuotedAndInvalidChar()
        {
            ServiceProvider
                .TestLine(@"  ExpectError ""Invalid token 'Paraeters'"".")
                .ValidateTokens()
                .ExpectError(@"ERROR - Invalid character at 41 '.'")
                .Assert();
        }

        [Test]
        public void TestAssignmentWithMemberAccess()
        {
            ServiceProvider
                .TestLine(@"  Value = ValidateEnumKeyword.Second")
                .ValidateTokens()
                .Count(3)
                .StringLiteral(0, "Value")
                .Operator(1, OperatorType.Assignment)
                .MemberAccess(2, "ValidateEnumKeyword.Second")
                .Assert();
        }

        [Test]
        public void TestAssignmentWithDoubleMemberAccess()
        {
            ServiceProvider
                .TestLine(@"  Value = ValidateEnumKeyword..Second", false)
                .ValidateTokens()
                .ExpectError("ERROR - Unexpected character: '.'. Member accessor should be followed by member name.")
                .Assert();
        }

        [Test]
        public void TestAssignmentWithMemberAccessWithoutLastMember()
        {
            ServiceProvider
                .TestLine(@"  Value = ValidateEnumKeyword.", false)
                .ValidateTokens()
                .ExpectError("ERROR - Invalid token at end of line. Unexpected end of line. Member accessor should be followed by member name.")
                .Assert();
        }
    }
}