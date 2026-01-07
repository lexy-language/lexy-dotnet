using Lexy.Compiler.Parser.Tokens;
using NUnit.Framework;

namespace Lexy.Tests.Tokenizer;

public class FunctionCallTests : ScopedServicesTestFixture
{
    [Test]
    public void TestWithArgument()
    {
        ServiceProvider
            .Tokenize(@"   lookUp(SimpleTable, 5, ""Result"")")
            .Count(8)
            .StringLiteral(0, "lookUp")
            .Operator(1, OperatorType.OpenParentheses)
            .StringLiteral(2, "SimpleTable")
            .Operator(3, OperatorType.ArgumentSeparator)
            .NumberLiteral(4, 5)
            .Operator(5, OperatorType.ArgumentSeparator)
            .QuotedString(6, "Result")
            .Operator(7, OperatorType.CloseParentheses)
            .Assert();
    }

    [Test]
    public void TestWithParametersSpreadOperator()
    {
        ServiceProvider
            .Tokenize(@"   lookUp(...)")
            .Count(4)
            .StringLiteral(0, "lookUp")
            .Operator(1, OperatorType.OpenParentheses)
            .Operator(2, OperatorType.Spread)
            .Operator(3, OperatorType.CloseParentheses)
            .Assert();
    }

    [Test]
    public void TestWithResultsSpreadOperator()
    {
        ServiceProvider
            .Tokenize(@"... = lookUp()")
            .Count(5)
            .Operator(0, OperatorType.Spread)
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "lookUp")
            .Operator(3, OperatorType.OpenParentheses)
            .Operator(4, OperatorType.CloseParentheses)
            .Assert();
    }

    [Test]
    public void TestWithParametersAndResultsSpreadOperator()
    {
        ServiceProvider
            .Tokenize(@"... = lookUp(...)")
            .Count(6)
            .Operator(0, OperatorType.Spread)
            .Operator(1, OperatorType.Assignment)
            .StringLiteral(2, "lookUp")
            .Operator(3, OperatorType.OpenParentheses)
            .Operator(4, OperatorType.Spread)
            .Operator(5, OperatorType.CloseParentheses)
            .Assert();
    }
}