using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public class SpreadAssignmentExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void FunctionCall()
    {
        var expression = this.ParseExpression("... = Function1()");
        expression.ValidateOfType<SpreadAssignmentExpression>(assignmentExpression =>
        {
            assignmentExpression.Assignment.ValidateOfType<LexyFunctionCallExpression>(functionCall =>
            {
                functionCall.FunctionName.ShouldBe("Function1");
            });
        });
    }

    [Test]
    public void FunctionCallWithSpreadExpression()
    {
        var expression = this.ParseExpression("... = Function1(...)");
        expression.ValidateOfType<SpreadAssignmentExpression>(assignmentExpression =>
        {
            assignmentExpression.Assignment.ValidateOfType<LexyFunctionCallExpression>(functionCall =>
            {
                functionCall.FunctionName.ShouldBe("Function1");
            });
        });
    }
}