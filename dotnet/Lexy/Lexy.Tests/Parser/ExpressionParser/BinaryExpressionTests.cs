using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class BinaryExpressionTests : ScopedServicesTestFixture
    {
        [Test]
        public void Addition()
        {
            var expression = this.ParseExpression("B + C");
            expression.ValidateOfType<BinaryExpression>(addition =>
            {
                addition.Operator.ShouldBe(ExpressionOperator.Addition);
                addition.Left.ValidateVariableExpression("B");
                addition.Right.ValidateVariableExpression("C");
            });
        }

        [Test]
        public void Subtraction()
        {
            var expression = this.ParseExpression("B - C");
            expression.ValidateOfType<BinaryExpression>(addition =>
            {
                addition.Operator.ShouldBe(ExpressionOperator.Subtraction);
                addition.Left.ValidateVariableExpression("B");
                addition.Right.ValidateVariableExpression("C");
            });
        }

        [Test]
        public void AdditionAndMultiplication()
        {
            var expression = this.ParseExpression("B + C * 12");
            expression.ValidateOfType<BinaryExpression>(addition =>
            {
                addition.Operator.ShouldBe(ExpressionOperator.Addition);
                addition.Left.ValidateVariableExpression("B");
                addition.Right.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.Multiplication);
                    multiplication.Left.ValidateVariableExpression("C");
                    multiplication.Right.ValidateNumericLiteralExpression(12m);
                });
            });
        }

        [Test]
        public void DivisionTests()
        {
            var expression = this.ParseExpression("B / 12");
            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.Division);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void ModulusTests()
        {
            var expression = this.ParseExpression("B % 12");
            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.Modulus);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void GreaterThan()
        {
            var expression = this.ParseExpression("B > 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.GreaterThan);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void GreaterThanOrEqual()
        {
            var expression = this.ParseExpression("B >= 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.GreaterThanOrEqual);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }


        [Test]
        public void LessThan()
        {
            var expression = this.ParseExpression("B < 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.LessThan);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void LessThanOrEqual()
        {
            var expression = this.ParseExpression("B <= 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.LessThanOrEqual);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void Equals()
        {
            var expression = this.ParseExpression("B == 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.Equals);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void NotEqual()
        {
            var expression = this.ParseExpression("B != 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.NotEqual);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void And()
        {
            var expression = this.ParseExpression("B && 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.And);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }

        [Test]
        public void Or()
        {
            var expression = this.ParseExpression("B || 12");

            expression.ValidateOfType<BinaryExpression>(multiplication =>
            {
                multiplication.Operator.ShouldBe(ExpressionOperator.Or);
                multiplication.Left.ValidateVariableExpression("B");
                multiplication.Right.ValidateNumericLiteralExpression(12m);
            });
        }
    }
}