using System.Collections.Generic;
using System.Linq;
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

    public VariablesMapping ParametersMapping { get; private set; }
    public VariableType ResultsObjectType { get; private set; }

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

        if (result is ValidateFunctionArgumentsAutoMapResult autoMapResult)
        {
            AutoMapParameters(context, autoMapResult.ParameterType);
        }

        ResultsObjectType = function.GetResultsType();
    }

    private void AutoMapParameters(IValidationContext context, VariableType functionParametersType)
    {
        if (functionParametersType is GeneratedType objectType)
        {
            ParametersMapping = FillParametersFunctionExpression.GetMapping(Reference, context, objectType);
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
        var result = base.UsedVariables();
        if (ParametersMapping != null)
        {
            result = result.Union(ParametersMapping.UsedVariables(VariableAccess.Read));
        }
        return result;
    }
}