using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public class FunctionCallExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void FunctionCallExpression()
    {
        var expression = this.ParseExpression("int(y)");
        expression.ValidateOfType<FunctionCallExpression>(functionCallExpression =>
        {
            functionCallExpression.FunctionName.ShouldBe("int");
            functionCallExpression.ValidateOfType<IntFunction>(function =>
                function.ValueExpression.ValidateVariableExpression("y"));
        });
    }

    [Test]
    public void NestedParenthesizedExpression()
    {
        var expression = this.ParseExpression("int(5 * (3 + A))");
        expression.ValidateOfType<FunctionCallExpression>(functionCall =>
        {
            functionCall.FunctionName.ShouldBe("int");
            functionCall.ValidateOfType<IntFunction>(function =>
                function.ValueExpression.ValidateOfType<BinaryExpression>(multiplication =>
                    multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                        inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                            addition.Operator.ShouldBe(ExpressionOperator.Addition)))));
        });
    }

    [Test]
    public void NestedParenthesizedMultipleArguments()
    {
        var expression = this.ParseExpression("round(power(98.6,3.2),3)");
        expression.ValidateOfType<RoundFunction>(round =>
        {
            round.FunctionName.ShouldBe("round");
            round.NumberExpression.ValidateOfType<PowerFunction>(power =>
            {
                power.FunctionName.ShouldBe("power");
                power.NumberExpression.ValidateNumericLiteralExpression(98.6m);
                power.PowerExpression.ValidateNumericLiteralExpression(3.2m);
            });
            round.DigitsExpression.ValidateNumericLiteralExpression(3);
        });
    }

    [Test]
    public void CallExtract()
    {
        var expression = this.ParseExpression("extract(result)");
        expression.ValidateOfType<ExtractResultsFunction>(round =>
        {
            round.FunctionName.ShouldBe("extract");
            round.ValueExpression.ValidateIdentifierExpression("result");
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