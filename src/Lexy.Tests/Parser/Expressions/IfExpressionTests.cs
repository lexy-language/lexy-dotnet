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

        Verify.Model(function, context => context
            .IsNotNull(value => value, valueContext => valueContext
                .Collection(value => value.Code.Expressions, expressionsContext => expressionsContext
                    .Length(3, "value.Code.Expressions")
                    .ValueModelOfType<IfExpression>(1, ifExpressionContext => ifExpressionContext
                        .Collection(value => value.TrueExpressions, trueExpressionContext => trueExpressionContext
                            .Length(1, "value.TrueExpressions")
                            .ValueModelOfType<AssignmentExpression>(0, assignmentExpression => assignmentExpression
                                .AreEqual(assignment => assignment.ToString(), "temp = 666")
                            )
                        )
                    )
                )
            )
        );
    }
}
