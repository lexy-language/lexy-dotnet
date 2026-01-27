using System;
using System.Diagnostics;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lexy.Tests.Tokenizer;

public static class TokenizerTestExtensions
{
    public static TokenValidator Tokenize(this IServiceProvider serviceProvider, string value)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var tokenizer = new Lexy.Compiler.Parser.Tokens.Tokenizer();

        var line = new Line(0, value, "tests.lexy");
        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException("Process line failed: " + tokens.ErrorMessage);
        }

        var logger = serviceProvider.GetRequiredService<ILogger<ParserLogger>>();
        var parserLogger = new ParserLogger(logger);
        var methodInfo = new StackTrace()?.GetFrame(1)?.GetMethod();

        return new TokenValidator(methodInfo?.ReflectedType?.Name, line, parserLogger);
    }

    public static TokenizeResult TokenizeExpectError(this IServiceProvider serviceProvider, string value)
    {
        Assert.NotNull(serviceProvider, nameof(serviceProvider));

        var tokenizer = serviceProvider.GetRequiredService<ITokenizer>();

        var line = new Line(0, value, "tests.lexy");
        var tokenizeResult = line.Tokenize(tokenizer);
        if (tokenizeResult.IsSuccess)
        {
            throw new InvalidOperationException( "Tokenizing didn't fail, but should have: " + tokenizeResult.ErrorMessage);
        }

        return tokenizeResult;
    }
}
