using System;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;
using Shouldly;

namespace Lexy.Tests.Parser.ExpressionParser;

public static class ParseExpressionTestExtensions
{
    public static Expression ParseExpression(this ScopedServicesTestFixture fixture, string expression)
    {
        var expressionFactory = new ExpressionFactory();
        var tokenizer = new Lexy.Compiler.Parser.Tokens.Tokenizer();

        var sourceFile = new SourceFile("tests.lexy");
        var line = new Line(0, expression, sourceFile);

        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException($"Tokenizing failed: {tokens.ErrorMessage}");
        }

        var result = expressionFactory.Parse(line.Tokens, line);
        result.IsSuccess.ShouldBeTrue(result.ErrorMessage);
        return result.Result;
    }

    public static void ParseExpressionExpectException(this ScopedServicesTestFixture fixture,
        string expression,
        string errorMessage)
    {
        var expressionFactory = new ExpressionFactory();
        var tokenizer = new Lexy.Compiler.Parser.Tokens.Tokenizer();
        var sourceFile = new SourceFile("tests.lexy");
        var line = new Line(0, expression, sourceFile);

        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException($"Tokenizing failed: {tokens.ErrorMessage}");
        }

        var result = expressionFactory.Parse(line.Tokens, line);
        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage.ShouldBe(errorMessage);
    }
}