using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Functions;

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