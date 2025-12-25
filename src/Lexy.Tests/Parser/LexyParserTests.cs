using Lexy.Compiler.Language.VariableTypes;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class LexyParserTests : ScopedServicesTestFixture
{
    [Test]
    public void TestSimpleReturn()
    {
        const string code = @"function TestSimpleReturn
  results
    number Result
  Code
    Result = 777";

        var (function, _) = ServiceProvider.ParseFunction(code);

        function.Name.Value.ShouldBe("TestSimpleReturn");
        function.Results.Variables.Count.ShouldBe(1);
        function.Results.Variables[0].Name.ShouldBe("Result");
        function.Results.Variables[0].Type.ValidateOfType<PrimitiveVariableDeclarationType>(type =>
            type.Type.ShouldBe("number"));
        function.Code.Expressions.Count.ShouldBe(1);
        function.Code.Expressions[0].ToString().ShouldBe("Result=777");
    }

    [Test]
    public void TestFunctionKeywords()
    {
        const string code = @"function ValidateFunctionKeywords
// Validate function keywords
  parameters
  results
  Code";

        var (_, logger) = ServiceProvider.ParseFunction(code);
        logger.HasErrors().ShouldBeFalse(logger.ToString());
    }
}