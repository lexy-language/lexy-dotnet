using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public async Task TestDuplicatedFunctionName()
    {
        const string code = @"function ValidateTableKeyword
  results
    number Result
  Result = 2

function ValidateTableKeyword
  results
    number Result
  Result = 2";

        var(_, logger, _) = await ServiceProvider.ParseNodes(code);

        logger.HasErrorMessage("Duplicated node name: 'ValidateTableKeyword'")
          .ShouldBeTrue(logger.FormatMessages());
    }


    [Test]
    public async Task TestWithFunctionDependencyAfterDependant()
    {
      const string code = @"function Calling
  parameters
    number Value
  results
    number Result
    string Message
  Result = Value + 7

function Caller
  parameters
    number Value
  results
    number Result
  Calling.Parameters params
  params.Value = Value
  ... = Calling(params)";

      var(_, logger, _) = await ServiceProvider.ParseNodes(code);

      logger.AssertNoErrors();
    }
}
