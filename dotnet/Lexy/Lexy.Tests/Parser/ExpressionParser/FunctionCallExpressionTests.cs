using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser;

public class FunctionCallExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void FunctionCallExpression()
    {
        var expression = this.ParseExpression("INT(y)");
        expression.ValidateOfType<FunctionCallExpression>(functionCallExpression =>
        {
            functionCallExpression.FunctionName.ShouldBe("INT");
            functionCallExpression.ExpressionFunction.ValidateOfType<IntFunction>(function =>
                function.ValueExpression.ValidateVariableExpression("y"));
        });
    }

    [Test]
    public void NestedParenthesizedExpression()
    {
        var expression = this.ParseExpression("INT(5 * (3 + A))");
        expression.ValidateOfType<FunctionCallExpression>(functionCall =>
        {
            functionCall.FunctionName.ShouldBe("INT");
            functionCall.ExpressionFunction.ValidateOfType<IntFunction>(function =>
                function.ValueExpression.ValidateOfType<BinaryExpression>(multiplication =>
                    multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                        inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                            addition.Operator.ShouldBe(ExpressionOperator.Addition)))));
        });
    }

    [Test]
    public void NestedParenthesizedMultipleArguments()
    {
        var expression = this.ParseExpression("ROUND(POWER(98.6,3.2),3)");
        expression.ValidateOfType<FunctionCallExpression>(round =>
        {
            round.FunctionName.ShouldBe("ROUND");
            round.Arguments.Count.ShouldBe(2);
            round.Arguments[0].ValidateOfType<FunctionCallExpression>(power =>
            {
                power.Arguments.Count.ShouldBe(2);
                power.Arguments[0].ValidateNumericLiteralExpression(98.6m);
                power.Arguments[1].ValidateNumericLiteralExpression(3.2m);
            });
            round.Arguments[1].ValidateNumericLiteralExpression(3);
        });
    }

    [Test]
    public void CallExtract()
    {
        var expression = this.ParseExpression("extract(results)");
        expression.ValidateOfType<FunctionCallExpression>(round =>
        {
            round.FunctionName.ShouldBe("extract");
            round.Arguments.Count.ShouldBe(1);
            round.Arguments[0].ValidateIdentifierExpression("results");
        });
    }

    [Test]
    public void InvalidParenthesizedExpression()
    {
        this.ParseExpressionExpectException(
            "func(A",
            "(FunctionCallExpression) No closing parentheses found.");
    }


    [Test]
    public void InvalidNestedParenthesizedExpression()
    {
        this.ParseExpressionExpectException(
            "func(5 * (3 + A)",
            "(FunctionCallExpression) No closing parentheses found.");
    }
}