using System;

namespace Lexy.Compiler.Language.VariableTypes.Functions;

public class ValidateInstanceFunctionArgumentsResult
{
    private readonly IMemberFunctionCall functionCall;

    public IMemberFunctionCall FunctionCall
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

    private ValidateInstanceFunctionArgumentsResult(bool isSuccess, IMemberFunctionCall functionCall)
    {
        IsSuccess = isSuccess;
        this.functionCall = functionCall;
    }

    public static ValidateInstanceFunctionArgumentsResult Failed()
    {
        return new ValidateInstanceFunctionArgumentsResult(false);
    }

    public static ValidateInstanceFunctionArgumentsResult Success(IMemberFunctionCall functionCall)
    {
        return new ValidateInstanceFunctionArgumentsResult(true, functionCall);
    }
}