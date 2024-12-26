using Lexy.Poc.Core.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser
{
    public class BracketedExpressionTests : ScopedServicesTestFixture
    {
        [Test]
        public void FunctionCallExpression()
        {
            var expression = this.ParseExpression("func[y]");
            expression.ValidateOfType<BracketedExpression>(functionCallExpression =>
            {
                functionCallExpression.FunctionName.ShouldBe("func");
                functionCallExpression.Expression.ValidateVariableExpression("y");
            });
        }

        [Test]
        public void NestedParenthesizedExpression()
        {
            var expression = this.ParseExpression("func[5 * (3 + A)]");
            expression.ValidateOfType<BracketedExpression>(functionCall =>
            {
                functionCall.FunctionName.ShouldBe("func");
                functionCall.Expression.ValidateOfType<BinaryExpression>(multiplication =>
                {
                    multiplication.Operator.ShouldBe(ExpressionOperator.Multiplication);
                    multiplication.Right.ValidateOfType<ParenthesizedExpression>(inner =>
                        inner.Expression.ValidateOfType<BinaryExpression>(addition =>
                            addition.Operator.ShouldBe(ExpressionOperator.Addition)));
                });
            });
        }

        [Test]
        public void InvalidParenthesizedExpression()
        {
            this.ParseExpressionExpectException(
                "func[A",
                "(BracketedExpression) No closing bracket found.");
        }


        [Test]
        public void InvalidNestedParenthesizedExpression()
        {
            this.ParseExpressionExpectException(
                "func[5 * [3 + A]",
                "(BracketedExpression) No closing bracket found.");
        }
    }
}