using Lexy.Compiler.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public void TestDuplicatedFunctionName()
    {
        const string code = @"function ValidateTableKeyword
  results
    number Result
  Code
    Result = 2

function ValidateTableKeyword
  results
    number Result
  Code
    Result = 2";

        var(_, logger) = ServiceProvider.ParseNodes(code);

        logger.HasErrorMessage("Duplicated node name: 'ValidateTableKeyword'")
          .ShouldBeTrue(logger.FormatMessages());
    }
}