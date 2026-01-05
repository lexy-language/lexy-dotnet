using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class ParseScenarioTests : ScopedServicesTestFixture
{
    [Test]
    public void TestValidScenarioKeyword()
    {
        const string code = @"scenario TestScenario";

        var (scenario, _) = ServiceProvider.ParseScenario(code);

        scenario.Name.Value.ShouldBe("TestScenario");
    }

    [Test]
    public void TestValidScenario()
    {
        const string code = @"scenario TestScenario
  function TestScenarioFunction
  parameters
    Value = 123
  results
    Result = 456";

        var (scenario, _) = ServiceProvider.ParseScenario(code);

        scenario.Name.Value.ShouldBe("TestScenario");
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
    public void TestInvalidScenario()
    {
        const string code = @"scenario TestScenario
  Functtion TestScenarioFunction
  parameters
    Value = 123
  results
    Result = 456";

        var (scenario, logger) = ServiceProvider.ParseScenario(code);

        var errors = logger.ErrorNodeMessages(scenario);

        logger.NodeHasErrors(scenario).ShouldBeTrue();

        errors.Length.ShouldBe(4, logger.ErrorMessages().Format(2));
        errors[0].ShouldBe("tests.lexy(1, 1): ERROR - Scenario has no function, enum, table or expect errors.");
        errors[1].ShouldBe("tests.lexy(2, 3): ERROR - Invalid token 'Functtion'. Keyword expected.");
        errors[2].ShouldBe("tests.lexy(4, 5): ERROR - Invalid identifier: 'Value'");
        errors[3].ShouldBe("tests.lexy(6, 5): ERROR - Invalid identifier: 'Result'");
    }

    [Test]
    public void TestInvalidNumberValueScenario()
    {
        const string code = @"scenario TestScenario
  function
    results
      number Result
  parameters
    Value = 12d3
  results
    Result = 456";

        var (scenario, logger) = ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario).ShouldBeTrue();

        var errors = logger.ErrorNodeMessages(scenario);
        errors.Length.ShouldBe(1, logger.FormatMessages());
        errors[0].ShouldBe("tests.lexy(6, 15): ERROR - Invalid number token character: 'd'");
    }

    [Test]
    public void TestScenarioWithInlineFunction()
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

        var (scenario, _) = ServiceProvider.ParseScenario(code);

        scenario.Name.Value.ShouldBe("ValidNumberIntAsParameter");
        scenario.Function.ShouldNotBeNull();
        scenario.Function.Parameters.Variables.Count.ShouldBe(2);
        scenario.Function.Parameters.Variables[0].Name.ShouldBe("Value1");
        scenario.Function.Parameters.Variables[0].Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(value =>
            ShouldBeStringTestExtensions.ShouldBe(value.Type, "number"));
        scenario.Function.Parameters.Variables[0].DefaultExpression.ToString().ShouldBe("123");
        scenario.Function.Parameters.Variables[1].Name.ShouldBe("Value2");
        scenario.Function.Parameters.Variables[1].Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(value =>
            value.Type.ShouldBe("number"));
        scenario.Function.Parameters.Variables[1].DefaultExpression.ToString().ShouldBe("456");
        scenario.Function.Results.Variables.Count.ShouldBe(2);
        scenario.Function.Results.Variables[0].Name.ShouldBe("Result1");
        scenario.Function.Results.Variables[0].Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(value =>
            value.Type.ShouldBe("number"));
        scenario.Function.Results.Variables[0].DefaultExpression.ShouldBeNull();
        scenario.Function.Results.Variables[1].Name.ShouldBe("Result2");
        scenario.Function.Results.Variables[1].Type.ValidateOfType<PrimitiveVariableTypeDeclaration>(value =>
            value.Type.ShouldBe("number"));
        scenario.Function.Results.Variables[1].DefaultExpression.ShouldBeNull();
        scenario.Function.Code.Expressions.Count.ShouldBe(2);
        scenario.Function.Code.Expressions[0].ToString().ShouldBe("Result1=Value1");
        scenario.Function.Code.Expressions[1].ToString().ShouldBe("Result2=Value2");

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
    public void TestScenarioWithEmptyParametersAndResults()
    {
        const string code = @"scenario ValidateScenarioKeywords
// Validate Scenario keywords
  function ValidateFunctionKeywords
  parameters
  results";

        var (scenario, _) = ServiceProvider.ParseScenario(code);

        scenario.FunctionName.Value.ShouldBe("ValidateFunctionKeywords");
        scenario.Parameters.AllAssignments().Count.ShouldBe(0);
        scenario.Results.AllAssignments().Count.ShouldBe(0);
    }

    [Test]
    public void TestValidScenarioWithInvalidInlineFunction()
    {
        const string code = @"scenario InvalidNumberEndsWithLetter
  function
    results
      number Result
    Result = 123A

  expectErrors 
    ""Invalid token at 18: Invalid number token character: A""";

        var (scenario, logger) = ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario).ShouldBeFalse();
        logger.NodeHasErrors(scenario.Function).ShouldBeTrue();

        scenario.Function.ShouldNotBeNull();
        scenario.ExpectErrors.ShouldNotBeNull();
    }

    [Test]
    public void ScenarioWithInlineFunctionShouldHaveAFunctionNameAfterKeywords()
    {
        const string code = @"scenario TestScenario
  function ThisShouldBeAllowed";

        var (scenario, logger) = ServiceProvider.ParseScenario(code);

        logger.HasErrors().ShouldBeFalse();
        logger.NodeHasErrors(scenario).ShouldBeFalse();
    }

    [Test]
    public void ScenarioWithInlineFunctionShouldLogErrorOnFunction()
    {
        const string code = @"scenario TestScenario
  function
    scenario";

        var (scenario, logger) = ServiceProvider.ParseScenario(code);

        logger.NodeHasErrors(scenario.Function).ShouldBeTrue();

        var errors = logger.ErrorNodeMessages(scenario.Function);
        errors.Length.ShouldBe(1);
        errors[0].ShouldContain("Invalid expression: KeywordToken('scenario')");
    }
}