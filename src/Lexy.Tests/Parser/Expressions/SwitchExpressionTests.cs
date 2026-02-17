using System;
using System.Threading.Tasks;
using Lexy.Compiler.Language.Expressions;
using NUnit.Framework;
using Shouldly;
using SwitchExpression = Lexy.Compiler.Language.Expressions.SwitchExpression;

namespace Lexy.Tests.Parser.Expressions;

public class SwitchExpressionTests : ScopedServicesTestFixture
{
    [Test]
    public async Task CheckSwitchStatement()
    {
        const string code = @"function NumberSwitch
  parameters
    number Evil
  results
    number Number
  number temp = 555
  switch Evil
    case 6
      temp = 666
    case 7
      temp = 777
    default
      temp = 888
  Number = temp";

        var (function, logger) = await ServiceProvider.ParseFunction(code);
        logger.AssertNoErrors();

        function.ShouldNotBeNull();
        Verify.Model(function, context => context
            .Collection(value => value.Code.Expressions, expressionContext => expressionContext
                .Length(3, "value.Code.Expressions")
                .ValueModelOfType<SwitchExpression>(1, switchExpression => switchExpression
                    .Collection(expression => expression.Cases, casesContext => casesContext
                        .Length(3, "value.Code.Expressions[1].Cases")
                        .ValueModel(0, CheckCase("number: 6", "temp = 666"))
                        .ValueModel(1, CheckCase("number: 7", "temp = 777"))
                        .ValueModel(2, CheckCase(null, "temp = 888"))
                    )
                )
            )
        );
    }

    private static Action<VerifyModelContext<CaseExpression>> CheckCase(string literal, string assignment) => context => context
        .AreEqual(value => value.Value != null ? value.Value.ToString() : null, literal)
        .Collection(value => value.Expressions, expressionContext => expressionContext
            .ValueAt(0, value => value != null && value.ToString() == assignment)
        );
}
