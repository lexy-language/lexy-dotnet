using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Tokenizer;

public class KeywordTests : ScopedServicesTestFixture
{
    [Test]
    public void TestFunctionKeyword()
    {
        ServiceProvider
            .Tokenize("function TestSimpleReturn")
            .Count(2)
            .Keyword(0, "function")
            .StringLiteral(1, "TestSimpleReturn")
            .Assert();
    }

    [Test]
    public void TestResultKeyword()
    {
        ServiceProvider
            .Tokenize("  results")
            .Count(1)
            .Keyword(0, "results")
            .Assert();
    }

    [Test]
    public void TestExpectErrorKeywordWithQuotedLiteral()
    {
        ServiceProvider
            .Tokenize(@"  expectErrors ""Invalid token 'Paraeters'""")
            .Count(2)
            .Keyword(0, "expectErrors")
            .QuotedString(1, "Invalid token 'Paraeters'")
            .Assert();
    }

    [Test]
    public void TestExpectErrorKeywordWithQuotedAndInvalidChar()
    {
        ServiceProvider
            .TokenizeExpectError(@"  expectError ""Invalid token 'Paraeters'""$")
            .ErrorMessage.ShouldContain(@"Invalid character at 41 '$'");
    }

    [Test]
    public void TestIncompleteSpreadOperator()
    {
        ServiceProvider
            .TokenizeExpectError(@"  .")
            .ErrorMessage.ShouldContain(@"Invalid token at end of line. Incomplete token: '.'");
    }

    [Test]
    public void TestAssignmentWithMemberAccess()
    {
        ServiceProvider
            .Tokenize(@"  Value = ValidateEnumKeyword.Second")
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
            .TokenizeExpectError(@"  Value = ValidateEnumKeyword..Second")
            .ErrorMessage.ShouldContain("Unexpected character: '.'. Member accessor should be followed by member name.");
    }

    [Test]
    public void TestAssignmentWithMemberAccessWithoutLastMember()
    {
        ServiceProvider
            .TokenizeExpectError(@"  Value = ValidateEnumKeyword.")
            .ErrorMessage.ShouldContain("Invalid token at end of line. Unexpected end of line. Member accessor should be followed by member name.");
    }
}