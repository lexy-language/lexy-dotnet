using System.Threading.Tasks;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem.Declaration;
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

        Verify.Model(function, context => context
            .AreEqual(value => value.Name, "TestSimpleReturn")
            .Collection(value => value.Results.Variables, variablesContext => variablesContext
                .Length(1, "value.Results.Variables")
                .ValueModel(0, value => value, itemContext => itemContext
                    .AreEqual(item => item.Name, "Result")
                    .IsOfType<ValueTypeDeclaration>(item => item.TypeDeclaration, typeDeclarationContext => typeDeclarationContext
                        .AreEqual(typeDeclaration => typeDeclaration.TypeName, "number"))))
            .Collection(value => function.Code.Expressions, variablesContext => variablesContext
                .Length(1, "function.Code.Expressions")
                .ValueModelOfType<AssignmentExpression>(0, itemContext => itemContext
                    .AreEqual(item => item.ToString(), "Result = 777"))));
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
