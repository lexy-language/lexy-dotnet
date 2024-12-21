using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Compiler
{

    public class LexyScriptTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestSimpleReturn()
        {
            var script = ServiceScope.CompileFunction(@"Function: TestSimpleReturn
  Results
    number Result
  Code
    Result = 777");
            var result = script.Run();
            result.Number("Result").ShouldBe(777);
        }

        [Test]
        public void TestParameterDefaultReturn()
        {
            var script = ServiceScope.CompileFunction(@"Function: TestSimpleReturn
  Parameters
    number Input = 5
  Results
    number Result
  Code
    Result = Input");
            var result = script.Run();
            result.Number("Result").ShouldBe(5);
        }

        [Test]
        public void TestAssignmentReturn()
        {
            var script = ServiceScope.CompileFunction(@"Function: TestSimpleReturn
  Parameters
    number Input = 5

  Results
    number Result
  Code
    Result = Input");
            var result = script.Run(new Dictionary<string, object>
            {
                { "Input", 777 }
            });
            result.Number("Result").ShouldBe(777);
        }
    }
}