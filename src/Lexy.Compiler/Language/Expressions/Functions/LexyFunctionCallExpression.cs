using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCallExpression : FunctionCallExpression, IHasNodeDependencies
{
    private readonly IList<Mapping> mappingParameters = new List<Mapping>();
    private readonly IList<Mapping> mappingResults = new List<Mapping>();

    public string FunctionName { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    public IEnumerable<Mapping> MappingParameters => mappingParameters;
    public IEnumerable<Mapping> MappingResults => mappingResults;

    public string ParameterName { get; private set; }
    public bool AutoMap { get; private set; }
    public VariableType FunctionParametersTypes { get; private set; }
    public VariableType FunctionResultsType { get; private set; }

    public LexyFunctionCallExpression(string functionName, IReadOnlyList<Expression> arguments, ExpressionSource source) : base(source)
    {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
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

        var result = function.ValidateArguments(context, Arguments);
        if (!result.IsSuccess) return;

        if (result.AutoMap)
        {
            AutoMap = true;
            FunctionResultsType = result.ResultType;
            FunctionParametersTypes = result.ParameterType;
            AutoMapVariables(context, result.ParameterType, result.ResultType);
        }

        ParameterName = GetParameterName();
    }

    private void AutoMapVariables(IValidationContext context, VariableType functionParametersType, VariableType functionResultsType)
    {
        if (functionParametersType is GeneratedType complexParameterType)
        {
            FillParametersFunctionExpression.GetMapping(Reference, context, complexParameterType, mappingParameters);
        }

        if (functionResultsType is GeneratedType complexResultsType)
        {
            ExtractResultsFunctionExpression.GetMapping(Reference, context, complexResultsType, mappingResults);
        }
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
        return base.UsedVariables()
            .Union(mappingParameters.Select(map => map.ToUsedVariable(VariableAccess.Read))
            .Union(mappingResults.Select(map => map.ToUsedVariable(VariableAccess.Write))));
    }

    private string GetParameterName()
    {
        if (Arguments.Count == 0 || Arguments[0] is not IdentifierExpression expressionArgument)
        {
            return null;
        }

        return expressionArgument.Identifier;
    }
}
