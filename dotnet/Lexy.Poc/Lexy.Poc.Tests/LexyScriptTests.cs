using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc
{
    public class LexyScriptTests
    {
        [Test]
        public void TestSimpleReturn()
        {
            var script = LexyScript.Create(@"Function: Test simple return
  Result
    int Result
  Code
    Result = 777");
            var result = script.Run();
            result.Int("Result").ShouldBe(777);
        }

        [Test]
        public void TestParameterDefaultReturn()
        {
            var script = LexyScript.Create(@"Function: Test simple return
  Parameters
    int Input = 5
  Result
    int Result
  Code
    Result = Input");
            var result = script.Run();
            result.Int("Result").ShouldBe(5);
        }

        [Test]
        public void TestAssignmentReturn()
        {
            var script = LexyScript.Create(@"Function: Test simple return
  Parameters
    int Input = 5

  Result
    int Result
  Code
    Result = Input");
            var result = script.Run(new Dictionary<string, object>
            {
                { "Input", 777 }
            });
            result.Int("Result").ShouldBe(777);
        }
    }
}