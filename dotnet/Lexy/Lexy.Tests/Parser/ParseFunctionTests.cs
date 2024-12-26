using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class ParseFunctionTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestDuplicatedFunctionName()
        {
            var code = @"Function: ValidateTableKeyword
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

            var parser = GetService<ILexyParser>();
            parser.ParseNodes(code);

            var logger = GetService<IParserLogger>();
            logger.HasErrorMessage("Duplicated node name: 'ValidateTableKeyword'").ShouldBeTrue(logger.FormatMessages());
        }
    }
}