using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class ExpressionsOperatorsTests : ScopedServicesTestFixture
{
    [Test]
    public void TestAdditionOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = Y + 1")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "Y")
            .Operator(3, OperatorType.Addition)
            .NumberLiteral(4, 1)
            .Assert();
    }

    [Test]
    public void TestSubtractionOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = Y - 1")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "Y")
            .Operator(3, OperatorType.Subtraction)
            .NumberLiteral(4, 1)
            .Assert();
    }

    [Test]
    public void TestMultiplicationOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = Y * 1")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "Y")
            .Operator(3, OperatorType.Multiplication)
            .NumberLiteral(4, 1)
            .Assert();
    }

    [Test]
    public void TestDivisionOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = Y / 1")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "Y")
            .Operator(3, OperatorType.Division)
            .NumberLiteral(4, 1)
            .Assert();
    }

    [Test]
    public void TestModulusOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = Y % 1")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "Y")
            .Operator(3, OperatorType.Modulus)
            .NumberLiteral(4, 1)
            .Assert();
    }

    [Test]
    public void TestParenthesesOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = (Y)")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .Operator(2, OperatorType.OpenParentheses)
            .StringLiteral(3, "Y")
            .Operator(4, OperatorType.CloseParentheses)
            .Assert();
    }

    [Test]
    public void TestBracketsOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = A[1]")
            .ValidateTokens()
            .Count(6)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "A")
            .Operator(3, OperatorType.OpenBrackets)
            .NumberLiteral(4, 1)
            .Operator(5, OperatorType.CloseBrackets)
            .Assert();
    }

    [Test]
    public void TestLessThanOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = A < 7")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "A")
            .Operator(3, OperatorType.LessThan)
            .NumberLiteral(4, 7)
            .Assert();
    }

    [Test]
    public void TestLessOrEqualThanOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = A <= 7")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "A")
            .Operator(3, OperatorType.LessThanOrEqual)
            .NumberLiteral(4, 7)
            .Assert();
    }

    [Test]
    public void TestGreaterThanOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = A > 7")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "A")
            .Operator(3, OperatorType.GreaterThan)
            .NumberLiteral(4, 7)
            .Assert();
    }

    [Test]
    public void TestGreaterOrEqualThanOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = A >= 7")
            .ValidateTokens()
            .Count(5)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "A")
            .Operator(3, OperatorType.GreaterThanOrEqual)
            .NumberLiteral(4, 7)
            .Assert();
    }

    [Test]
    public void TestFunctionCallOperator()
    {
        ServiceProvider
            .Tokenize(@"  X = abs(Y + 8)")
            .ValidateTokens()
            .Count(8)
            .StringLiteral(0, "X")
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "abs")
            .Operator(3, OperatorType.OpenParentheses)
            .StringLiteral(4, "Y")
            .Operator(5, OperatorType.Addition)
            .NumberLiteral(6, 8)
            .Operator(7, OperatorType.CloseParentheses)
            .Assert();
    }
}