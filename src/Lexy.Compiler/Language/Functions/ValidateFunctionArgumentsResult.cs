using System;
using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Functions;

public class ValidateFunctionArgumentsResult
{
    private readonly VariableType parameterType;
    private readonly VariableType resultType;
    private readonly bool autoMap;

    public VariableType ParameterType
    {
        get
        {
            if (!IsSuccess || !AutoMap) throw new InvalidOperationException($"Can't get ParameterType, IsSuccess: {IsSuccess} AutoMap: {AutoMap}");
            return parameterType;
        }
    }

    public VariableType ResultType
    {
        get
        {
            if (!IsSuccess || !AutoMap) throw new InvalidOperationException($"Can't get ResultType, IsSuccess: {IsSuccess} AutoMap: {AutoMap}");
            return resultType;
        }
    }

    public bool AutoMap
    {
        get
        {
            if (!IsSuccess) throw new InvalidOperationException($"Can't get AutoMap, IsSuccess: {IsSuccess}");
            return autoMap;
        }
    }

    public bool IsSuccess { get; }

    private ValidateFunctionArgumentsResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    private ValidateFunctionArgumentsResult(VariableType parameterType, VariableType resultType, bool autoMap)
    {
        IsSuccess = true;

        this.autoMap = autoMap;
        this.parameterType = parameterType;
        this.resultType = resultType;
    }

    public static ValidateFunctionArgumentsResult Failed()
    {
        return new ValidateFunctionArgumentsResult(false);
    }

    public static ValidateFunctionArgumentsResult SuccessAutoMap(VariableType parameterType, VariableType resultType)
    {
        return new ValidateFunctionArgumentsResult(parameterType, resultType, true);
    }

    public static ValidateFunctionArgumentsResult Success(VariableType resultType)
    {
        return new ValidateFunctionArgumentsResult(null, resultType, false);
    }
}