using Lexy.Poc.Core.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class OperationOrderTests : ScopedServicesTestFixture
    {
        [Test]
        public void AddAndMultiply()
        {
            var expression = this.ParseExpression("a + b * c");
            expression.ValidateOfType<BinaryExpression>(add =>
            {
                add.Operator.ShouldBe(ExpressionOperator.Addition);
                add.Left.ValidateVariableExpression("a");
                add.Right.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.Multiplication);
                    multiplication.Left.ValidateVariableExpression("b");
                    multiplication.Right.ValidateVariableExpression("c");
                });
            });
        }

        [Test]
        public void AddAndMultiplyReverse()
        {
            var expression = this.ParseExpression("a * b + c");
            expression.ValidateOfType<BinaryExpression>(add =>
            {
                add.Operator.ShouldBe(ExpressionOperator.Addition);
                add.Left.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.Multiplication);
                    multiplication.Left.ValidateVariableExpression("a");
                    multiplication.Right.ValidateVariableExpression("b");
                });
                add.Right.ValidateVariableExpression("c");
            });
        }

        [Test]
        public void AndAndOr()
        {
            var expression = this.ParseExpression("a && b || c");
            expression.ValidateOfType<BinaryExpression>(add =>
            {
                add.Operator.ShouldBe(ExpressionOperator.Or);
                add.Left.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.And);
                    multiplication.Left.ValidateVariableExpression("a");
                    multiplication.Right.ValidateVariableExpression("b");
                });
                add.Right.ValidateVariableExpression("c");
            });
        }

        [Test]
        public void OrAndAn()
        {
            var expression = this.ParseExpression("a && b || c");
            expression.ValidateOfType<BinaryExpression>(add =>
            {
                add.Operator.ShouldBe(ExpressionOperator.Or);
                add.Left.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.And);
                    multiplication.Left.ValidateVariableExpression("a");
                    multiplication.Right.ValidateVariableExpression("b");
                });
                add.Right.ValidateVariableExpression("c");
            });
        }
    }
}