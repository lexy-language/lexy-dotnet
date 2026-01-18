using System;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

public class ValidateMemberFunctionArgumentsResult
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

    private ValidateMemberFunctionArgumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private ValidateMemberFunctionArgumentsResult(bool isSuccess, IMemberFunctionCall functionCall)
    {
        IsSuccess = isSuccess;
        this.functionCall = functionCall;
    }

    public static ValidateMemberFunctionArgumentsResult Failed()
    {
        return new ValidateMemberFunctionArgumentsResult(false);
    }

    public static ValidateMemberFunctionArgumentsResult Success(IMemberFunctionCall functionCall)
    {
        return new ValidateMemberFunctionArgumentsResult(true, functionCall);
    }
}