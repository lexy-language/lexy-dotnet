using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public class FunctionCallExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void FunctionCallExpression()
    {
        var expression = this.ParseExpression("int(y)");
        expression.ValidateOfType<LexyFunctionCallExpression>(functionCallExpression =>
        {
            functionCallExpression.FunctionName.ShouldBe("int");
            functionCallExpression.Arguments.Count.ShouldBe(1);
            functionCallExpression.Arguments[0].ValidateVariableExpression("y");
        });
    }

    [Test]
    public void NestedParenthesizedExpression()
    {
        var expression = this.ParseExpression("int(5 * (3 + A))");
        expression.ValidateOfType<LexyFunctionCallExpression>(functionCall =>
        {
            functionCall.FunctionName.ShouldBe("int");
            functionCall.Arguments.Count.ShouldBe(1);
            functionCall.Arguments[0].ValidateOfType<BinaryExpression>(multiplication =>
                multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                    inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                        addition.Operator.ShouldBe(ExpressionOperator.Addition))));
        });
    }

    [Test]
    public void NestedParenthesizedMultipleArguments()
    {
        var expression = this.ParseExpression("round(power(98.6,3.2),3)");
        expression.ValidateOfType<LexyFunctionCallExpression>(round =>
        {
            round.FunctionName.ShouldBe("round");
            round.Arguments.Count.ShouldBe(2);
            round.Arguments[0].ValidateOfType<LexyFunctionCallExpression>(power =>
            {
                power.FunctionName.ShouldBe("power");
                power.Arguments.Count.ShouldBe(2);
                power.Arguments[0].ValidateNumericLiteralExpression(98.6m);
                power.Arguments[1].ValidateNumericLiteralExpression(3.2m);
            });
            round.Arguments[1].ValidateNumericLiteralExpression(3);
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

      [Test]
    public void LibraryFunctionCallExpression()
    {
        var expression = this.ParseExpression("Number.Parse(y)");
        expression.ValidateOfType<MemberFunctionCallExpression>(functionCallExpression =>
        {
            functionCallExpression.FunctionPath.FullPath().ShouldBe("Number.Parse");
            functionCallExpression.Arguments.Count.ShouldBe(1);
            functionCallExpression.Arguments[0].ValidateVariableExpression("y");
        });
    }

    [Test]
    public void LibraryFunctionCallNestedParenthesizedExpression()
    {
        var expression = this.ParseExpression("Number.Parse(5 * (3 + A))");
        expression.ValidateOfType<MemberFunctionCallExpression>(functionCall =>
        {
            functionCall.FunctionPath.FullPath().ShouldBe("Number.Parse");
            functionCall.Arguments.Count.ShouldBe(1);
            functionCall.Arguments[0].ValidateOfType<BinaryExpression>(multiplication =>
                multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                    inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                        addition.Operator.ShouldBe(ExpressionOperator.Addition))));
        });
    }

    [Test]
    public void LibraryFunctionCallNestedParenthesizedMultipleArguments()
    {
        var expression = this.ParseExpression("Number.Round(Math.Power(98.6,3.2),3)");
        expression.ValidateOfType<MemberFunctionCallExpression>(round =>
        {
            round.FunctionPath.FullPath().ShouldBe("Number.Round");
            round.Arguments.Count.ShouldBe(2);
            round.Arguments[0].ValidateOfType<MemberFunctionCallExpression>(power =>
            {
                power.FunctionPath.FullPath().ShouldBe("Math.Power");
                power.Arguments.Count.ShouldBe(2);
                power.Arguments[0].ValidateNumericLiteralExpression(98.6m);
                power.Arguments[1].ValidateNumericLiteralExpression(3.2m);
            });
            round.Arguments[1].ValidateNumericLiteralExpression(3);
        });
    }

    [Test]
    public void LibraryFunctionCallInvalidNestedParenthesizedExpression()
    {
        this.ParseExpressionExpectException(
            "Math.Func(A",
            "(FunctionCallExpression) No closing parentheses found.");
    }

    [Test]
    public void CallExtract()
    {
        var expression = this.ParseExpression("extract(result)");
        expression.ValidateOfType<ExtractResultsFunction>(round =>
        {
            round.ValueExpression.ValidateIdentifierExpression("result");
        });
    }


    [Test]
    public void CallFill()
    {
        var expression = this.ParseExpression("fill(result)");
        expression.ValidateOfType<FillParametersFunction>(round =>
        {
            round.ValueExpression.ValidateIdentifierExpression("result");
        });
    }


    [Test]
    public void CallNew()
    {
        var expression = this.ParseExpression("new(result)");
        expression.ValidateOfType<NewFunction>(round =>
        {
            round.ValueExpression.ValidateIdentifierExpression("result");
        });
    }
}