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
            .Tokenize("Function: TestSimpleReturn")
            .Count(2)
            .Keyword(0, "Function:")
            .StringLiteral(1, "TestSimpleReturn")
            .Assert();
    }

    [Test]
    public void TestResultKeyword()
    {
        ServiceProvider
            .Tokenize("  Results")
            .Count(1)
            .Keyword(0, "Results")
            .Assert();
    }

    [Test]
    public void TestExpectErrorKeywordWithQuotedLiteral()
    {
        ServiceProvider
            .Tokenize(@"  ExpectErrors ""Invalid token 'Paraeters'""")
            .Count(2)
            .Keyword(0, "ExpectErrors")
            .QuotedString(1, "Invalid token 'Paraeters'")
            .Assert();
    }

    [Test]
    public void TestExpectErrorKeywordWithQuotedAndInvalidChar()
    {
        ServiceProvider
            .TokenizeExpectError(@"  ExpectError ""Invalid token 'Paraeters'"".")
            .ErrorMessage.ShouldContain(@"Invalid character at 41 '.'");
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