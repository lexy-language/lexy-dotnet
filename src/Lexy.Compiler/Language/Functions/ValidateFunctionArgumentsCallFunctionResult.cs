namespace Lexy.Compiler.Language.Functions;

public class ValidateFunctionArgumentsCallFunctionResult : ValidateFunctionArgumentsResult
{
    public FunctionSignature Function { get; }

    private ValidateFunctionArgumentsCallFunctionResult(FunctionSignature functionFunction) : base(true)
    {
        Function = functionFunction;
    }

    public static ValidateFunctionArgumentsCallFunctionResult Success(FunctionSignature functionSignature)
    {
        return new ValidateFunctionArgumentsCallFunctionResult(functionSignature);
    }
}