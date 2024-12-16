using Lexy.Poc.Core.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc
{
    public class LexyParserTests
    {
        [Test]
        public void TestSimpleReturn()
        {
            var code = @"Function: Test simple return
  Result
    number Result
  Code
    Result = 777";

            var parser = new LexyParser();
            var script = parser.ParseFunction(code);

            script.Name.Value.ShouldBe("Test simple return");
            script.Result.Variables.Count.ShouldBe(1);
            script.Result.Variables[0].Name.ShouldBe("Result");
            script.Result.Variables[0].Type.ShouldBe("number");
            script.Code.Lines.Count.ShouldBe(1);
            script.Code.Lines[0].ShouldBe("Result = 777");
        }

        [Test]
        public void TestFunctionKeywords()
        {
            var code = @"Function: ValidateFunctionKeywords
# Validate function keywords
  Parameters
  Result
  Code";

            var parser = new LexyParser();
            parser.ParseFunction(code);

        }
    }
}