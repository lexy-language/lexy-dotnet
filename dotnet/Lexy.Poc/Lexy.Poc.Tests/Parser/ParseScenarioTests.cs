using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Poc.Parser
{
    public class ParseScenarioTests : ScopedServicesTestFixture
    {
        [Test]
        public void TestValidScenarioKeyword()
        {
            const string code = @"Scenario: TestScenario";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            scenario.Name.Value.ShouldBe("TestScenario");
        }

        [Test]
        public void TestValidScenario()
        {
            const string code = @"Scenario: TestScenario
  Function TestScenarioFunction
  Parameters
    Value = 123
  Results
    Result = 456";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            scenario.Name.Value.ShouldBe("TestScenario");
            scenario.FunctionName.Value.ShouldBe("TestScenarioFunction");
            scenario.Parameters.Assignments.Count.ShouldBe(1);
            scenario.Parameters.Assignments[0].Name.ShouldBe("Value");
            scenario.Parameters.Assignments[0].Expression.ToString().ShouldBe("123");
            scenario.Results.Assignments.Count.ShouldBe(1);
            scenario.Results.Assignments[0].Name.ShouldBe("Result");
            scenario.Results.Assignments[0].Expression.ToString().ShouldBe("456");
        }

        [Test]
        public void TestInvalidScenario()
        {
            const string code = @"Scenario: TestScenario
  Functtion TestScenarioFunction
  Parameters
    Value = 123
  Results
    Result = 456";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            var context = GetService<IParserContext>();
            context.Logger.NodeHasErrors(scenario).ShouldBeTrue();

            var errors = context.Logger.NodeFailedMessages(scenario);
            errors.Length.ShouldBe(1);
            errors[0].ShouldBe("tests.lexy(2, 2): ERROR - Invalid token 'Functtion'. Keyword expected.");
        }

        [Test]
        public void TestInvalidNumberValueScenario()
        {
            const string code = @"Scenario: TestScenario
  Function TestScenarioFunction
  Parameters
    Value = 12d3
  Results
    Result = 456";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            var context = GetService<IParserContext>();
            context.Logger.NodeHasErrors(scenario).ShouldBeTrue();

            var errors = context.Logger.NodeFailedMessages(scenario);
            errors.Length.ShouldBe(1, context.Logger.FormatMessages());
            errors[0].ShouldBe("tests.lexy(4, 15): ERROR - Invalid token at 14: Invalid number token character: d");
        }

        [Test]
        public void TestScenarioWithInlineFunction()
        {
            const string code = @"Scenario: ValidNumberIntAsParameter
  Function: ValidNumberIntAsParameterFunction
    Parameters
      number Value1 = 123
      number Value2 = 456
    Results
      number Result1
      number Result2
    Code
      Result1 = Value1
      Result2 = Value2
  Parameters
    Value1 = 987
    Value2 = 654
  Results
    Result1 = 123
    Result2 = 456";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            scenario.Name.Value.ShouldBe("ValidNumberIntAsParameter");
            scenario.Function.ShouldNotBeNull();
            scenario.Function.Parameters.Variables.Count.ShouldBe(2);
            scenario.Function.Parameters.Variables[0].Name.ShouldBe("Value1");
            scenario.Function.Parameters.Variables[0].Type.ShouldBeOfType<PrimitiveVariableDeclarationType>();
            (scenario.Function.Parameters.Variables[0].Type as PrimitiveVariableDeclarationType).Type.ShouldBe("number");
            scenario.Function.Parameters.Variables[0].Default.ToString().ShouldBe("123");
            scenario.Function.Parameters.Variables[1].Name.ShouldBe("Value2");
            scenario.Function.Parameters.Variables[1].Type.ShouldBeOfType<PrimitiveVariableDeclarationType>();
            (scenario.Function.Parameters.Variables[1].Type as PrimitiveVariableDeclarationType).Type.ShouldBe("number");
            scenario.Function.Parameters.Variables[1].Default.ToString().ShouldBe("456");
            scenario.Function.Results.Variables.Count.ShouldBe(2);
            scenario.Function.Results.Variables[0].Name.ShouldBe("Result1");
            scenario.Function.Results.Variables[0].Type.ShouldBeOfType<PrimitiveVariableDeclarationType>();
            (scenario.Function.Results.Variables[0].Type as PrimitiveVariableDeclarationType).Type.ShouldBe("number");
            scenario.Function.Results.Variables[0].Default.ShouldBeNull();
            scenario.Function.Results.Variables[1].Name.ShouldBe("Result2");
            scenario.Function.Results.Variables[1].Type.ShouldBeOfType<PrimitiveVariableDeclarationType>();
            (scenario.Function.Results.Variables[1].Type as PrimitiveVariableDeclarationType).Type.ShouldBe("number");
            scenario.Function.Results.Variables[1].Default.ShouldBeNull();
            scenario.Function.Code.Expressions.Count.ShouldBe(2);
            scenario.Function.Code.Expressions[0].ToString().ShouldBe("Result1=Value1");
            scenario.Function.Code.Expressions[1].ToString().ShouldBe("Result2=Value2");

            scenario.Parameters.Assignments.Count.ShouldBe(2);
            scenario.Parameters.Assignments[0].Name.ShouldBe("Value1");
            scenario.Parameters.Assignments[0].Expression.ToString().ShouldBe("987");
            scenario.Parameters.Assignments[1].Name.ShouldBe("Value2");
            scenario.Parameters.Assignments[1].Expression.ToString().ShouldBe("654");

            scenario.Results.Assignments.Count.ShouldBe(2);
            scenario.Results.Assignments[0].Name.ShouldBe("Result1");
            scenario.Results.Assignments[0].Expression.ToString().ShouldBe("123");
            scenario.Results.Assignments[1].Name.ShouldBe("Result2");
            scenario.Results.Assignments[1].Expression.ToString().ShouldBe("456");
        }

        [Test]
        public void TestScenarioWithEmptyParametersAndResults()
        {
            const string code = @"Scenario: ValidateScenarioKeywords
# Validate Scenario keywords
  Function ValidateFunctionKeywords
  Parameters
  Results";
            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            scenario.FunctionName.Value.ShouldBe("ValidateFunctionKeywords");
            scenario.Parameters.Assignments.Count.ShouldBe(0);
            scenario.Results.Assignments.Count.ShouldBe(0);
        }

        [Test]
        public void TestValidScenarioWithInvalidInlineFunction()
        {
            const string code = @"Scenario: InvalidNumberEndsWithLetter
  Function: InvalidNumberEndsWithLetterFunction
    Results
      number Result
    Code
      Result = 123A
  ExpectError ""Invalid token at 18: Invalid number token character: A""";

            var parser = GetService<ILexyParser>();
            var scenario = parser.ParseScenario(code);

            var logger = GetService<IParserLogger>();

            logger.NodeHasErrors(scenario).ShouldBeFalse();
            logger.NodeHasErrors(scenario.Function).ShouldBeTrue();

            scenario.Function.ShouldNotBeNull();
            scenario.ExpectError.ShouldNotBeNull();
        }
    }
}