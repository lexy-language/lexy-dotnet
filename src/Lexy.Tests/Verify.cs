using System;

namespace Lexy.Tests;

public class Verify<TModel>
{
    private readonly Context<TModel> context;
    private readonly VerifyLogging logging = new();

    private Verify(TModel model)
    {
        context = new Context<TModel>(model, logging);
    }

    public static void Model(TModel model, Action<Context<TModel>> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var verify = new Verify<TModel>(model);
        verify.Execute<TModel>(testHandler);
        verify.VerifyAll();
    }

    private void Execute<TFactory>(Action<Context<TModel>> testHandler)
    {
        testHandler(context);
    }

    private void VerifyAll()
    {
        var summary = logging.ToString();
        if (logging.Errors)
        {
            throw new InvalidOperationException(summary);
        }

        Console.WriteLine(summary);
    }
}