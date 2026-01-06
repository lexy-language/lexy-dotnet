using System;

namespace Lexy.Compiler.Language.Functions;

public class ValidateFunctionArgumentsResult
{
    public bool IsSuccess { get; }

    protected ValidateFunctionArgumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public static ValidateFunctionArgumentsResult Failed()
    {
        return new ValidateFunctionArgumentsResult(false);
    }
}