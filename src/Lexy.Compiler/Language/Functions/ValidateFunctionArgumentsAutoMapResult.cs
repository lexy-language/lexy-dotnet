using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language.Functions;

public class ValidateFunctionArgumentsAutoMapResult : ValidateFunctionArgumentsResult
{
    public Type ParameterType { get; }

    private ValidateFunctionArgumentsAutoMapResult(Type parameterType): base(true)
    {
        ParameterType = parameterType;
    }

    public static ValidateFunctionArgumentsAutoMapResult SuccessAutoMap(Type parameterType, Type resultType)
    {
        return new ValidateFunctionArgumentsAutoMapResult(parameterType);
    }
}
