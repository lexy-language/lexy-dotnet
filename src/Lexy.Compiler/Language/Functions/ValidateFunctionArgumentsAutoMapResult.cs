using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Language.Functions;

public class ValidateFunctionArgumentsAutoMapResult : ValidateFunctionArgumentsResult
{
    public VariableType ParameterType { get; }

    private ValidateFunctionArgumentsAutoMapResult(VariableType parameterType, VariableType resultType): base(true)
    {
        ParameterType = parameterType;
    }

    public static ValidateFunctionArgumentsAutoMapResult SuccessAutoMap(VariableType parameterType, VariableType resultType)
    {
        return new ValidateFunctionArgumentsAutoMapResult(parameterType, resultType);
    }
}