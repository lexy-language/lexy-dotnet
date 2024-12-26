using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser.Tokens;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class AssignmentExpressionTests : ScopedServicesTestFixture
    {
        [Test]
        public void Addition()
        {
            var expression = this.ParseExpression("A = B + C");
            expression.ValidateOfType<AssignmentExpression>(assignmentExpression =>
            {
                assignmentExpression.VariableName.ShouldBe("A");
                assignmentExpression.Assignment.ValidateOfType<BinaryExpression>(addition =>
                {
                    addition.Operator.ShouldBe(ExpressionOperator.Addition);
                    addition.Left.ValidateVariableExpression("B");
                    addition.Right.ValidateVariableExpression("C");
                });
            });
        }

        [Test]
        public void AdditionAndMultiplication()
        {
            var expression = this.ParseExpression("A = B + C * 12");
            expression.ValidateOfType<AssignmentExpression>(assignment =>
            {
                assignment.VariableName.ShouldBe("A");
                assignment.Assignment.ValidateOfType<BinaryExpression>(addition =>
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
            });
        }
    }
}