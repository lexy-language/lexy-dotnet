using System;
using System.Diagnostics;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Tests.Tokenizer;

public static class TokenizerTestExtensions
{
    public static TokenValidator Tokenize(this IServiceProvider serviceProvider, string value)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        var tokenizer = new Lexy.Compiler.Parser.Tokens.Tokenizer();
        var file = new SourceFile("tests.lexy");
        var line = new Line(0, value, file);
        var tokens = line.Tokenize(tokenizer);
        if (!tokens.IsSuccess)
        {
            throw new InvalidOperationException("Process line failed: " + tokens.ErrorMessage);
        }

        var logger = serviceProvider.GetRequiredService<IParserLogger>();

        var parseLineContext = new ParseLineContext(line, logger);
        var methodInfo = new StackTrace()?.GetFrame(1)?.GetMethod();

        return parseLineContext.ValidateTokens(methodInfo?.ReflectedType?.Name);
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
}