using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser.Expressions;

public class SwitchExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public void CheckSwitchStatement()
    {
        const string code = @"Function: NumberSwitch
  Parameters
    number Evil
  Results
    number Number
  Code
    number temp = 555
    switch Evil
      case 6
        temp = 666
      case 7
        temp = 777
      default
        temp = 888
    Number = temp";

        var (function, logger) = ServiceProvider.ParseFunction(code);
        logger.AssertNoErrors();

        function.ShouldNotBeNull();
        function.Code.Expressions.Count.ShouldBe(3);
        function.Code.Expressions[1].ValidateOfType<SwitchExpression>(expression =>
        {
            var expressionCases = expression.Cases.ToArray();
            expressionCases.Count().ShouldBe(3);

            CheckCase(expressionCases, 0, "6", "temp=666");
            CheckCase(expressionCases, 1, "7", "temp=777");
            CheckCase(expressionCases, 2, null, "temp=888");
        });
    }

    private static void CheckCase(CaseExpression[] expressionCases, int index, string value, string expression)
    {
        expressionCases[index].Value?.ToString().ShouldBe(value);
        var expressions = expressionCases[index].Expressions.ToArray();
        expressions.Length.ShouldBe(1);
        expressions[0].ToString().ShouldBe(expression);
    }
}