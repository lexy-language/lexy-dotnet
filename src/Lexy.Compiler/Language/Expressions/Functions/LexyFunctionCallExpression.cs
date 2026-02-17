using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunctionCallExpression : FunctionCallExpression, IHasNodeDependencies
{
    public string FunctionName { get; }

    public IReadOnlyList<Expression> Arguments { get; }

    public LexyFunctionCallState State { get; private set; }

    public override string Name => FunctionName;

    public LexyFunctionCallExpression(string functionName, IReadOnlyList<Expression> arguments,
        NodeReference parentReference, ExpressionSource source) : base(parentReference, source)
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

        var parametersMapping = AutoMapParameters(result, context);
        var resultsObjectType = function.GetResultsType();
        var returnSingleResultsVariablesName = ReturnSingleResultsVariablesName(function);

        State = new LexyFunctionCallState(parametersMapping, resultsObjectType, returnSingleResultsVariablesName);
    }

    private VariablesMapping AutoMapParameters(ValidateFunctionArgumentsResult result, IValidationContext context)
    {
        if (result is not ValidateFunctionArgumentsAutoMapResult autoMapResult) return null;

        var functionParametersType = autoMapResult.ParameterType;
        if (functionParametersType is GeneratedType objectType)
        {
            return FillParametersFunctionExpression.GetMapping(Reference, context, objectType);
        }
        return null;
    }

    private Function GetFunction(IValidationContext context)
    {
        return context.ComponentNodes.GetFunction(FunctionName);
    }

    public override Type DeriveType(IValidationContext context)
    {
        var function = GetFunction(context);
        var variable = ReturnSingleResultsVariable(function);
        return variable != null ? variable.State.Type : function?.GetResultsType();
    }

    private string ReturnSingleResultsVariablesName(Function function)
    {
        var variable = ReturnSingleResultsVariable(function);
        return variable?.Name;
    }

    private VariableDefinition ReturnSingleResultsVariable(Function function)
    {
        var parentIsSpreadExpression = Parent is SpreadAssignmentExpression;
        return !parentIsSpreadExpression && function.Results.Variables.Count == 1
            ? function.Results.Variables[0]
            : null;
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        var result = base.UsedVariables();
        if (State?.ParametersMapping != null)
        {
            result = result.Union(State.ParametersMapping.UsedVariables(VariableAccess.Read));
        }
        return result;
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, $"function: {Name}", string.Empty, SymbolKind.Function);
    }
}
