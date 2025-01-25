using Lexy.Compiler.Language.Expressions;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public class MemberAccessExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void SimpleMemberAccess()
    {
        var expression = this.ParseExpression("A = B.C");
        expression.ValidateOfType<AssignmentExpression>(assignmentExpression =>
        {
            assignmentExpression.Variable.ValidateIdentifierExpression("A");
            assignmentExpression.Assignment.ValidateOfType<MemberAccessExpression>(memberAccess =>
                memberAccess.Variable.ToString().ShouldBe("B.C"));
        });
    }
}