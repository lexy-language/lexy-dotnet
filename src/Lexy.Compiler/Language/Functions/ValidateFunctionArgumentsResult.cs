using System;
using Lexy.Compiler.Language.VariableTypes;

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

public class ValidateFunctionArgumentsCallFunctionResult : ValidateFunctionArgumentsResult
{
    public IFunctionSignature Function { get; }

    private ValidateFunctionArgumentsCallFunctionResult(IFunctionSignature functionFunction) : base(true)
    {
        this.Function = functionFunction;
    }

    public static ValidateFunctionArgumentsCallFunctionResult Success(IFunctionSignature functionSignature)
    {
        return new ValidateFunctionArgumentsCallFunctionResult(functionSignature);
    }
}

public class ValidateFunctionArgumentsAutoMapResult : ValidateFunctionArgumentsResult
{
    public VariableType ParameterType { get; }

    public VariableType ResultType { get; }

    private ValidateFunctionArgumentsAutoMapResult(VariableType parameterType, VariableType resultType): base(true)
    {
        ParameterType = parameterType;
        ResultType = resultType;
    }

    public static ValidateFunctionArgumentsAutoMapResult SuccessAutoMap(VariableType parameterType, VariableType resultType)
    {
        return new ValidateFunctionArgumentsAutoMapResult(parameterType, resultType);
    }
}