using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Functions;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.FunctionLibraries;

internal class LibraryFunction : IObjectTypeFunction
{
    private readonly MemberInfo functionInfo;
    private readonly VariableType returnType;
    private readonly VariableType[] parameterTypes;

    public IdentifierPath FullTypeName { get; }

    private LibraryFunction(MemberInfo functionInfo, VariableType returnType, VariableType[] parameterTypes)
    {
        this.functionInfo = Assert.NotNull(functionInfo, nameof(functionInfo));
        this.returnType = returnType;
        this.parameterTypes = parameterTypes;

        FullTypeName =
            IdentifierPath.Parse(functionInfo.DeclaringType?.Namespace, functionInfo.DeclaringType?.Name, functionInfo.Name);
    }

    public ValidateMemberFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments,
        SourceReference reference)
    {
        if (arguments.Count != parameterTypes.Length)
        {
            context.Logger.Fail(reference, $"Invalid number of function arguments: '{functionInfo.Name}'. ");
            return ValidateMemberFunctionArgumentsResult.Failed();
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
            ? ValidateMemberFunctionArgumentsResult.Failed()
            : ValidateMemberFunctionArgumentsResult.Success(new LibraryFunctionCall(FullTypeName, returnType));
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