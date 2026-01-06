using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCallExpression : FunctionCallExpression, IHasNodeDependencies
{
    public string FunctionName { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    public ILexyFunctionCall FunctionCall { get; private set; }

    public LexyFunctionCallExpression(string functionName, IReadOnlyList<Expression> arguments, ExpressionSource source) : base(source)
    {
        FunctionName = Assert.NotNull(functionName, nameof(functionName));
        Arguments = Assert.NotNull(arguments, nameof(arguments));
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var component = componentNodes.GetNode(FunctionName);
        if (component != null) yield return component;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Arguments;
    }

    protected override void Validate(IValidationContext context)
    {
        var function = GetFunction(context);
        if (function == null)
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{FunctionName}'");
            return;
        }

        var result = function.ValidateArguments(context, Arguments, Reference);
        if (result is not { IsSuccess: true }) return;

        FunctionCall = result switch
        {
            ValidateFunctionArgumentsAutoMapResult autoMapResult => AutoMapVariables(context,
                autoMapResult.ParameterType, autoMapResult.ResultType),
            ValidateFunctionArgumentsCallFunctionResult argumentsCall => CallLexyFunction(argumentsCall),
            _ => throw new InvalidOperationException("Invalid ValidateArguments result: " + result.GetType())
        };
    }

    private ILexyFunctionCall AutoMapVariables(IValidationContext context, VariableType functionParametersType, VariableType functionResultsType)
    {
        var mappingParameters = new List<Mapping>();
        if (functionParametersType is GeneratedType complexParameterType)
        {
            FillParametersFunctionExpression.GetMapping(Reference, context, complexParameterType, mappingParameters);
        }

        var mappingResults = new List<Mapping>();
        if (functionResultsType is GeneratedType complexResultsType)
        {
            ExtractResultsFunctionExpression.GetMapping(Reference, context, complexResultsType, mappingResults);
        }

        return new AutoMapLexyFunctionCall(mappingParameters, mappingResults, functionParametersType, functionResultsType);
    }

    private ILexyFunctionCall CallLexyFunction(ValidateFunctionArgumentsCallFunctionResult result)
    {
        return new LexyFunctionCall(result.Function.ParametersTypes, result.Function.ResultsType, Arguments);
    }

    private Function GetFunction(IValidationContext context)
    {
        return context.ComponentNodes.GetFunction(FunctionName);
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        var function = GetFunction(context);
        return function?.GetResultsType();
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        if (FunctionCall == null) return base.UsedVariables();

        return base.UsedVariables()
            .Union(FunctionCall.UsedVariables());
    }
}