using System;
using System.Diagnostics;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Tokenizer;

public static class TokenizerTestExtensions
{
    public static IParserContext Tokenize(this IServiceProvider serviceProvider, string value)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        var code = new[] { value };

        var codeContext = serviceProvider.GetRequiredService<ISourceCodeDocument>();
        codeContext.SetCode(code, "tests.lexy");

        var tokenizer = serviceProvider.GetRequiredService<ITokenizer>();

        var line = codeContext.NextLine();
        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException("Process line failed: " + tokens.ErrorMessage);
        }

        return serviceProvider.GetRequiredService<IParserContext>();
    }

    public static TokenizeResult TokenizeExpectError(this IServiceProvider serviceProvider, string value)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        var code = new[] { value };

        var codeContext = serviceProvider.GetRequiredService<ISourceCodeDocument>();
        codeContext.SetCode(code, "tests.lexy");

        var tokenizer = serviceProvider.GetRequiredService<ITokenizer>();

        var line = codeContext.NextLine();
        var tokenizeResult = line.Tokenize(tokenizer);
        if (tokenizeResult.IsSuccess)
        {
            throw new InvalidOperationException( "Process didn't fail, but should have: " + tokenizeResult.ErrorMessage);
        }

        return tokenizeResult;
    }

    public static TokenValidator ValidateTokens(this IParserContext context)
    {
        var parseLineContext = new ParseLineContext(context.CurrentLine, context.Logger);
        var methodInfo = new StackTrace()?.GetFrame(1)?.GetMethod();
        return parseLineContext.ValidateTokens(methodInfo?.ReflectedType?.Name);
    }
}