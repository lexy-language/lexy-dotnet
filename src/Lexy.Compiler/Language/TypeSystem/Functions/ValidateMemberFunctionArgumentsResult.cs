using System;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

public class ValidateMemberFunctionArgumentsResult
{
    private readonly IFunctionCallState functionCallState;

    public IFunctionCallState FunctionCallState
    {
        get
        {
            if (!IsSuccess) throw new InvalidOperationException($"Can't get FunctionCall, IsSuccess: {IsSuccess}");
            return functionCallState;
        }
    }

    public bool IsSuccess { get; }

    private ValidateMemberFunctionArgumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private ValidateMemberFunctionArgumentsResult(bool isSuccess, IFunctionCallState functionCallState)
    {
        IsSuccess = isSuccess;
        this.functionCallState = functionCallState;
    }

    public static ValidateMemberFunctionArgumentsResult Failed()
    {
        return new ValidateMemberFunctionArgumentsResult(false);
    }

    public static ValidateMemberFunctionArgumentsResult Success(IFunctionCallState functionCallState)
    {
        return new ValidateMemberFunctionArgumentsResult(true, functionCallState);
    }
}