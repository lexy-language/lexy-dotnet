using System;
using System.Diagnostics;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Lexy.Poc
{
    public static class TestContext
    {
        public static IParserContext TestLine(this IServiceProvider serviceProvider, string value, bool expectSuccess = true)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

            var code = new []{ value };

            var codeContext = serviceProvider.GetRequiredService<ISourceCodeDocument>();
            codeContext.SetCode(code, "tests.lexy");

            var context = serviceProvider.GetRequiredService<IParserContext>();
            var result = context.ProcessLine();
            if (result != expectSuccess)
            {
                throw new InvalidOperationException(  result
                    ? "Process didn't fail, but should have: " + context.Logger.FormatMessages()
                    : "Process line failed: " + context.Logger.FormatMessages());
            }

            return context;
        }

        public static void ValidateError(this IParserContext context, string error)
        {
            context.Logger.HasErrorMessage(error)
                .ShouldBeTrue(context.Logger.FormatMessages());
        }

        public static TokenValidator ValidateTokens(this IParserContext context)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            return context.ValidateTokens(methodInfo.ReflectedType.Name);
        }

        public static void ExpectException(Action action, string errorMessage)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

            try
            {
                action();
            }
            catch (Exception e)
            {
                if (!e.Message.Contains(errorMessage))
                {
                    throw new InvalidOperationException(
                        $"Wrong error message. Expected '{errorMessage}' Actual: '{e.Message}'");
                }
                return;
            }
            throw new InvalidOperationException(
                $"No excepction thrown. Expected '{errorMessage}''");
        }
    }
}