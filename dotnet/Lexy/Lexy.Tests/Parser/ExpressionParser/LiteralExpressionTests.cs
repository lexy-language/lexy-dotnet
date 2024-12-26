using Lexy.Poc.Core.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class LiteralExpressionTests : ScopedServicesTestFixture
    {
        [Test]
        public void Number()
        {
            var expression = this.ParseExpression("456");
            expression.ValidateNumericLiteralExpression(456);
        }

        [Test]
        public void NegativeNumber()
        {
            var expression = this.ParseExpression("-456");
            expression.ValidateNumericLiteralExpression(-456);
        }

        [Test]
        public void Subtraction()
        {
            var expression = this.ParseExpression("789-456");
            expression.ValidateOfType<BinaryExpression>(expression =>
            {
                expression.Operator.ShouldBe(ExpressionOperator.Subtraction);
                expression.Left.ValidateNumericLiteralExpression(789);
                expression.Right.ValidateNumericLiteralExpression(456);
            });
        }

        [Test]
        public void DoubleSubtraction()
        {
            var expression = this.ParseExpression("789 - -456");
            expression.ValidateOfType<BinaryExpression>(expression =>
            {
                expression.Operator.ShouldBe(ExpressionOperator.Subtraction);
                expression.Left.ValidateNumericLiteralExpression(789);
                expression.Right.ValidateNumericLiteralExpression(-456);
            });
        }

        [Test]
        public void DoubleSubtractionWithSpace()
        {
            var expression = this.ParseExpression("789 - -456");
            expression.ValidateOfType<BinaryExpression>(subtraction =>
            {
                subtraction.Operator.ShouldBe(ExpressionOperator.Subtraction);
                subtraction.Left.ValidateNumericLiteralExpression(789);
                subtraction.Right.ValidateNumericLiteralExpression(-456);
            });
        }

        [Test]
        public void FunctionCallWithNegativeNumber()
        {
            var expression = this.ParseExpression("Result = ABS(-2)");
            expression.ValidateOfType<AssignmentExpression>(assignment =>
            {
                assignment.VariableName.ShouldBe("Result");
                assignment.Assignment.ValidateOfType<FunctionCallExpression>(functionCall =>
                {
                    functionCall.FunctionName.ShouldBe("ABS");
                    functionCall.Arguments.Count.ShouldBe(1);
                    functionCall.Arguments[0].ValidateNumericLiteralExpression(-2);
                });
            });
        }
    }
}