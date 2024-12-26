using Lexy.Poc.Core.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class ParenthesizedExpressionTests : ScopedServicesTestFixture
    {
        [Test]
        public void ParenthesizedExpression()
        {
            var expression = this.ParseExpression("(A)");
            expression.ValidateOfType<ParenthesizedExpression>(parenthesized =>
                parenthesized.Expression.ValidateVariableExpression("A"));
        }

        [Test]
        public void NestedParenthesizedExpression()
        {
            var expression = this.ParseExpression("(5 * (3 + A))");
            expression.ValidateOfType<ParenthesizedExpression>(parenthesis =>
                parenthesis.Expression.ValidateOfType<BinaryExpression>(multiplication =>
                    multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                        inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                            addition.Operator.ShouldBe(ExpressionOperator.Addition)))));
        }

        [Test]
        public void InvalidParenthesizedExpression()
        {
            this.ParseExpressionExpectException(
                "(A",
                "(ParenthesizedExpression) No closing parentheses found.");
        }

        [Test]
        public void InvalidNestedParenthesizedExpression()
        {
            this.ParseExpressionExpectException(
                "(5 * (3 + A)",
                "(ParenthesizedExpression) No closing parentheses found.");
        }
    }
}