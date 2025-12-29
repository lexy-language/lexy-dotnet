using System;

namespace Lexy.Compiler.Language.Functions;

public class ValidateInstanceFunctionArgumentsResult
{
    private readonly IInstanceFunctionCall functionCall;

    public IInstanceFunctionCall FunctionCall
    {
        get
        {
            if (!IsSuccess) throw new InvalidOperationException($"Can't get FunctionCall, IsSuccess: {IsSuccess}");
            return functionCall;
        }
    }

    public bool IsSuccess { get; }

    private ValidateInstanceFunctionArgumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private ValidateInstanceFunctionArgumentsResult(bool isSuccess, IInstanceFunctionCall functionCall)
    {
        IsSuccess = isSuccess;
        this.functionCall = functionCall;
    }

    public static ValidateInstanceFunctionArgumentsResult Failed()
    {
        return new ValidateInstanceFunctionArgumentsResult(false);
    }

    public static ValidateInstanceFunctionArgumentsResult Success(IInstanceFunctionCall functionCall)
    {
        return new ValidateInstanceFunctionArgumentsResult(true, functionCall);
    }
}