using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.Expressions;

public class IfExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void CheckIfStatement()
    {
      const string code = @"Function: If
  Parameters
    boolean Evil
  Results
    number Number
  Code
    number temp = 777
    if Evil
      temp = 666
    Number = temp";

        var (function, logger) = ServiceProvider.ParseFunction(code);

        logger.AssertNoErrors();

        function.ShouldNotBeNull();
        function.Code.Expressions.Count.ShouldBe(3);
        function.Code.Expressions[1].ValidateOfType<IfExpression>(expression =>
        {
          expression.TrueExpressions.Count().ShouldBe(1);
          expression.TrueExpressions.ToArray()[0].ValidateOfType<AssignmentExpression>(assignment =>
            assignment.ToString().ShouldBe("temp=666"));
        });
    }
}
