using System;

namespace Lexy.Tests;

public class Verify<TModel>
{
    private readonly VerifyModelContext<TModel> verifyModelContext;
    private readonly VerifyLogging logging = new();

    private Verify(TModel model)
    {
        verifyModelContext = new VerifyModelContext<TModel>(model, logging);
    }

    public static void Model(TModel model, Action<VerifyModelContext<TModel>> testHandler)
    {
        if (testHandler == null) throw new ArgumentNullException(nameof(testHandler));

        var verify = new Verify<TModel>(model);
        verify.Execute<TModel>(testHandler);
        verify.VerifyAll();
    }

    private void Execute<TFactory>(Action<VerifyModelContext<TModel>> testHandler)
    {
        testHandler(verifyModelContext);
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