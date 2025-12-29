using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.FunctionLibraries;

internal class LibraryFunction : IInstanceFunction
{
    private readonly MemberInfo functionInfo;
    private readonly VariableType returnType;
    private readonly VariableType[] parameterTypes;

    public IdentifierPath FullTypeName { get; }

    private LibraryFunction(MemberInfo functionInfo, VariableType returnType, VariableType[] parameterTypes)
    {
        this.functionInfo = functionInfo ?? throw new ArgumentNullException(nameof(functionInfo));
        this.returnType = returnType;
        this.parameterTypes = parameterTypes;

        FullTypeName =
            IdentifierPath.Parse(functionInfo.DeclaringType?.Namespace, functionInfo.DeclaringType?.Name, functionInfo.Name);
    }

    public ValidateInstanceFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments,
        SourceReference reference)
    {
        if (arguments.Count != parameterTypes.Length)
        {
            context.Logger.Fail(reference, $"Invalid number of function arguments: '{functionInfo.Name}'. ");
            return ValidateInstanceFunctionArgumentsResult.Failed();
        }

        var failed = false;
        for (var index = 0; index < arguments.Count; index++)
        {
            if (!ValidateArgument(reference, context, arguments, index))
            {
                failed = true;
            }
        }

        return failed
            ? ValidateInstanceFunctionArgumentsResult.Failed()
            : ValidateInstanceFunctionArgumentsResult.Success(new LibraryFunctionCall(FullTypeName, returnType));
    }

    public VariableType GetResultsType(IReadOnlyList<Expression> arguments) => returnType;

    private bool ValidateArgument(SourceReference reference, IValidationContext context, IReadOnlyList<Expression> arguments, int index)
    {
        var argument = arguments[index];
        var argumentType = argument.DeriveType(context);

        var parametersType = parameterTypes[index];

        if (argumentType == null || !argumentType.Equals(parametersType))
        {
            context.Logger.Fail(reference, $"Invalid function argument: '{functionInfo.Name}'. " +
                                           "Argument should be of type function parameters. Use new(Function) of fill(Function) to create an variable of the function result type.");
            return false;
        }
        return true;
    }

    public static LibraryFunction Build(MethodInfo staticMethod)
    {
        var returnType = PrimitiveType.Parse(staticMethod.ReturnType);
        var parameterTypes = BuildParametersTypes(staticMethod.GetParameters());
        return new LibraryFunction(staticMethod, returnType, parameterTypes);
    }

    private static VariableType[] BuildParametersTypes(ParameterInfo[] parameters)
    {
        return parameters.Select(parameter => PrimitiveType.Parse(parameter.ParameterType)).ToArray();
    }
}

internal class LibraryFunctionCall : IInstanceFunctionCall
{
    public IdentifierPath FullTypeName { get; }
    public VariableType ReturnType { get; }

    public LibraryFunctionCall(IdentifierPath fullTypeName, VariableType returnType)
    {
        FullTypeName = fullTypeName;
        ReturnType = returnType;
    }
}