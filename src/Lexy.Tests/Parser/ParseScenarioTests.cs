using System.Threading.Tasks;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseScenarioTests : ScopedServicesTestFixture
{
    [Test]
    public async Task TestValidScenarioKeyword()
    {
        const string code = @"scenario TestScenario";

        var (scenario, _) = await ServiceProvider.ParseScenario(code);

        scenario.Name.ShouldBe("TestScenario");
    }

    [Test]
    public async Task TestValidScenario()
    {
        const string code = @"scenario TestScenario
  function TestScenarioFunction
  parameters
    Value = 123
  results
    Result = 456";

        var (scenario, _) = await ServiceProvider.ParseScenario(code);

        scenario.Name.ShouldBe("TestScenario");
        scenario.FunctionName.Value.ShouldBe("TestScenarioFunction");
        var parameterAssignments = scenario.Parameters.AllAssignments();
        parameterAssignments.Count.ShouldBe(1);
        parameterAssignments[0].Variable.RootIdentifier.ShouldBe("Value");
        parameterAssignments[0].ConstantValue.Value.ShouldBe(123m);
        var resultsAssignments = scenario.Results.AllAssignments();
        resultsAssignments.Count.ShouldBe(1);
        resultsAssignments[0].Variable.RootIdentifier.ShouldBe("Result");
        resultsAssignments[0].ConstantValue.Value.ShouldBe(456m);
    }

    [Test]
    public async Task TestInvalidScenario()
    {
        const string code = @"scenario TestScenario
  Functtion TestScenarioFunction
  parameters
    Value = 123
  results
    Result = 456";

        var (scenario, logger) = await ServiceProvider.ParseScenario(code);

        var errors = logger.ErrorNodeMessages(scenario);

        logger.NodeHasErrors(scenario).ShouldBeTrue();

        Verify.ComparableCollection<string>(errors, _ => _
            .Length(4, logger.ErrorMessages().Format(2))
            .ValueAtEquals(0, "tests.lexy (1:1-21): ERROR - Scenario has no function, enum, table or expect errors.")
            .ValueAtEquals(1, "tests.lexy (2:3-32): ERROR - Invalid token 'Functtion'. Keyword expected.")
            .ValueAtEquals(2, "tests.lexy (4:5-9): ERROR - Invalid identifier: 'Value'")
            .ValueAtEquals(3, "tests.lexy (6:5-10): ERROR - Invalid identifier: 'Result'")
        );
    }

    [Test]
    public async Task TestInvalidNumberValueScenario()
    {
        const string code = @"scenario TestScenario
  function
    results
      number Result
  parameters
    Value = 12d3
  results
    Result = 456";

        var (scenario, logger) = await ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario).ShouldBeTrue();

        var errors = logger.ErrorNodeMessages(scenario);
        errors.Length.ShouldBe(1, logger.FormatMessages());
        errors[0].ShouldBe("tests.lexy (6:15): ERROR - Invalid number token character: 'd'");
    }

    [Test]
    public async Task TestScenarioWithInlineFunction()
    {
        const string code = @"scenario ValidNumberIntAsParameter
  function
    parameters
      number Value1 = 123
      number Value2 = 456
    results
      number Result1
      number Result2
    Result1 = Value1
    Result2 = Value2
  parameters
    Value1 = 987
    Value2 = 654
  results
    Result1 = 123
    Result2 = 456";

        var (scenario, _) = await ServiceProvider.ParseScenario(code);

        scenario.Name.ShouldBe("ValidNumberIntAsParameter");
        scenario.Function.ShouldNotBeNull();
        scenario.Function.Parameters.Variables.Count.ShouldBe(2);
        scenario.Function.Parameters.Variables[0].Name.ShouldBe("Value1");
        scenario.Function.Parameters.Variables[0].TypeDeclaration.ValidateOfType<ValueTypeDeclaration>(value =>
            ShouldBeStringTestExtensions.ShouldBe(value.TypeName, "number"));
        scenario.Function.Parameters.Variables[0].DefaultExpression.ToString().ShouldBe("123");
        scenario.Function.Parameters.Variables[1].Name.ShouldBe("Value2");
        scenario.Function.Parameters.Variables[1].TypeDeclaration.ValidateOfType<ValueTypeDeclaration>(value =>
            value.TypeName.ShouldBe("number"));
        scenario.Function.Parameters.Variables[1].DefaultExpression.ToString().ShouldBe("456");
        scenario.Function.Results.Variables.Count.ShouldBe(2);
        scenario.Function.Results.Variables[0].Name.ShouldBe("Result1");
        scenario.Function.Results.Variables[0].TypeDeclaration.ValidateOfType<ValueTypeDeclaration>(value =>
            value.TypeName.ShouldBe("number"));
        scenario.Function.Results.Variables[0].DefaultExpression.ShouldBeNull();
        scenario.Function.Results.Variables[1].Name.ShouldBe("Result2");
        scenario.Function.Results.Variables[1].TypeDeclaration.ValidateOfType<ValueTypeDeclaration>(value =>
            value.TypeName.ShouldBe("number"));
        scenario.Function.Results.Variables[1].DefaultExpression.ShouldBeNull();
        scenario.Function.Code.Expressions.Count.ShouldBe(2);
        scenario.Function.Code.Expressions[0].ToString().ShouldBe("(AssignmentExpression) Result1 = Value1");
        scenario.Function.Code.Expressions[1].ToString().ShouldBe("(AssignmentExpression) Result2 = Value2");

        var parameterAssignments = scenario.Parameters.AllAssignments();
        parameterAssignments.Count.ShouldBe(2);
        parameterAssignments[0].Variable.RootIdentifier.ShouldBe("Value1");
        parameterAssignments[0].ConstantValue.Value.ShouldBe(987m);
        parameterAssignments[1].Variable.RootIdentifier.ShouldBe("Value2");
        parameterAssignments[1].ConstantValue.Value.ShouldBe(654m);

        var resultsAssignments = scenario.Results.AllAssignments();
        resultsAssignments.Count.ShouldBe(2);
        resultsAssignments[0].Variable.RootIdentifier.ShouldBe("Result1");
        resultsAssignments[0].ConstantValue.Value.ShouldBe(123m);
        resultsAssignments[1].Variable.RootIdentifier.ShouldBe("Result2");
        resultsAssignments[1].ConstantValue.Value.ShouldBe(456m);
    }

    [Test]
    public async Task TestScenarioWithEmptyParametersAndResults()
    {
        const string code = @"scenario ValidateScenarioKeywords
// Validate Scenario keywords
  function ValidateFunctionKeywords
  parameters
  results";

        var (scenario, _) = await ServiceProvider.ParseScenario(code);

        scenario.FunctionName.Value.ShouldBe("ValidateFunctionKeywords");
        scenario.Parameters.AllAssignments().Count.ShouldBe(0);
        scenario.Results.AllAssignments().Count.ShouldBe(0);
    }

    [Test]
    public async Task TestValidScenarioWithInvalidInlineFunction()
    {
        const string code = @"scenario InvalidNumberEndsWithLetter
  function
    results
      number Result
    Result = 123A

  expectErrors 
    ""Invalid token at 18: Invalid number token character: A""";

        var (scenario, logger) = await ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario).ShouldBeFalse();
        logger.NodeHasErrors(scenario.Function).ShouldBeTrue();

        scenario.Function.ShouldNotBeNull();
        scenario.ExpectErrors.ShouldNotBeNull();
    }

    [Test]
    public async Task ScenarioWithInlineFunctionShouldHaveAFunctionNameAfterKeywords()
    {
        const string code = @"scenario TestScenario
  function ThisShouldBeAllowed";

        var (scenario, logger) = await ServiceProvider.ParseScenario(code);

        logger.HasErrors().ShouldBeFalse();
        logger.NodeHasErrors(scenario).ShouldBeFalse();
    }

    [Test]
    public async Task ScenarioWithInlineFunctionShouldLogErrorOnFunction()
    {
        const string code = @"scenario TestScenario
  function
    scenario";

        var (scenario, logger) = await ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario.Function).ShouldBeTrue();

        var errors = logger.ErrorNodeMessages(scenario.Function);
        errors.Length.ShouldBe(1);
        errors[0].ShouldContain("Invalid expression: KeywordToken('scenario')");
    }
}
