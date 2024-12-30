using System;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Shouldly;

namespace Lexy.Poc.Parser.ExpressionParser;

public static class ParseExpressionTestExtensions
{
    public static Expression ParseExpression(this ScopedServicesTestFixture fixture, string expression)
    {
        var context = fixture.GetService<IParserContext>();
        var tokenizer = fixture.GetService<ITokenizer>();
        var sourceFile = new SourceFile("tests.lexy");
        var line = new Line(0, expression, sourceFile);

        var tokens = tokenizer.Tokenize(line);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException("Tokenizing failed: " + context.Logger.ErrorMessages().Format(2));
        }

        line.SetTokens(tokens.Result);

        var result = ExpressionFactory.Parse(line.Tokens, line);
        result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
        return result.Result;
    }

    public static void ParseExpressionExpectException(this ScopedServicesTestFixture fixture,
        string expression,
        string errorMessage)
    {
        var context = fixture.GetService<IParserContext>();
        var tokenizer = fixture.GetService<ITokenizer>();
        var sourceFile = new SourceFile("tests.lexy");
        var line = new Line(0, expression, sourceFile);

        var tokens = tokenizer.Tokenize(line);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException("Tokenizing failed: " + context.Logger.ErrorMessages().Format(2));
        }

        line.SetTokens(tokens.Result);

        var result = ExpressionFactory.Parse(line.Tokens, line);
        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(errorMessage);
    }
}