using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class LexyParserTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestSimpleReturn()
        {
            var code = @"Function: TestSimpleReturn
  Results
    number Result
  Code
    Result = 777";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            var script = parser.ParseFunction(code);

            script.Name.Value.ShouldBe("TestSimpleReturn");
            script.Results.Variables.Count.ShouldBe(1);
            script.Results.Variables[0].Name.ShouldBe("Result");
            script.Results.Variables[0].Type.ShouldBeOfType<PrimitiveVariableDeclarationType>();
            (script.Results.Variables[0].Type as PrimitiveVariableDeclarationType).Type.ShouldBe("number");
            script.Code.Expressions.Count.ShouldBe(1);
            script.Code.Expressions[0].ToString().ShouldBe("Result=777");
        }

        [Test]
        public void TestFunctionKeywords()
        {
            var code = @"Function: ValidateFunctionKeywords
# Validate function keywords
  Parameters
  Results
  Code";

            var parser = ServiceProvider.GetRequiredService<ILexyParser>();
            parser.ParseFunction(code);
        }
    }
}