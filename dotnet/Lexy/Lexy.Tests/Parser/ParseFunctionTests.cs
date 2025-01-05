using Lexy.Compiler.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseFunctionTests : ScopedServicesTestFixture
{
    [Test]
    public void TestDuplicatedFunctionName()
    {
        const string code = @"Function: ValidateTableKeyword
# Validate table keywords
  Include
    table ValidateTableKeyword
  Parameters
  Results
    number Result
  Code
    Result = ValidateTableKeyword.Count

Function: ValidateTableKeyword
# Validate table keywords
  Include
    table ValidateTableKeyword
  Parameters
  Results
    number Result
  Code
    Result = ValidateTableKeyword.Count";

        ServiceProvider.ParseNodes(code);

        var logger = GetService<IParserLogger>();
        logger.HasErrorMessage("Duplicated node name: 'ValidateTableKeyword'")
          .ShouldBeTrue(logger.FormatMessages());
    }
}