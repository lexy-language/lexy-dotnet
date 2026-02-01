using System.Threading.Tasks;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Tests.Parser.ExpressionParser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Parser;

public class LexyParserTests : ScopedServicesTestFixture
{
    [Test]
    public async Task TestSimpleReturn()
    {
        const string code = @"function TestSimpleReturn
  results
    number Result
  Result = 777";

        var (function, logger) = await ServiceProvider.ParseFunction(code);

        logger.HasErrors().ShouldBeFalse(logger.ToString());
        function.Name.ShouldBe("TestSimpleReturn");
        function.Results.Variables.Count.ShouldBe(1);
        function.Results.Variables[0].Name.ShouldBe("Result");
        function.Results.Variables[0].TypeDeclaration.ValidateOfType<ValueTypeDeclaration>(type =>
            type.TypeName.ShouldBe("number"));
        function.Code.Expressions.Count.ShouldBe(1);
        function.Code.Expressions[0].ToString().ShouldBe("(AssignmentExpression) Result = 777");
    }

    [Test]
    public async Task TestFunctionKeywords()
    {
        const string code = @"function ValidateFunctionKeywords
// Validate function keywords
  parameters
  results";

        var (_, logger) = await ServiceProvider.ParseFunction(code);
        logger.HasErrors().ShouldBeFalse(logger.ToString());
    }
}
