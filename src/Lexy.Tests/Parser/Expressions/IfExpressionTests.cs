using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Language.Expressions;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.Expressions;

public class IfExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public async Task CheckIfStatement()
    {
      const string code = @"
function If
  parameters
    boolean Evil

  results
    number Number

  number temp = 777
  if Evil
    temp = 666
  Number = temp";

        var (function, logger) = await ServiceProvider.ParseFunction(code);

        logger.AssertNoErrors();

        function.ShouldNotBeNull();
        function.Code.Expressions.Count.ShouldBe(3);
        function.Code.Expressions[1].ValidateOfType<IfExpression>(expression =>
        {
            expression.TrueExpressions.Count().ShouldBe(1);
            expression.TrueExpressions.ToArray()[0].ValidateOfType<AssignmentExpression>(assignment =>
                assignment.ToString().ShouldBe("(AssignmentExpression) temp = 666"));
        });
    }
}
