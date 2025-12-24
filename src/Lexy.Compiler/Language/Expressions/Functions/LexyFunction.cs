using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Expressions.Functions;

public class LexyFunction : FunctionCallExpression, IHasNodeDependencies
{
    private readonly IReadOnlyList<Expression> arguments;
    private readonly IList<Mapping> mappingParameters = new List<Mapping>();
    private readonly IList<Mapping> mappingResults = new List<Mapping>();

    public string VariableName { get; private set; }

    public IEnumerable<Mapping> MappingParameters => mappingParameters;
    public IEnumerable<Mapping> MappingResults => mappingResults;

    public ComplexType FunctionParametersType { get; private set; }
    public ComplexType FunctionResultsType { get; private set; }

    public LexyFunction(string functionName, IReadOnlyList<Expression> arguments, ExpressionSource source) : base(functionName, source)
    {
        this.arguments = arguments;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodeList)
    {
        var function = componentNodeList.GetFunction(FunctionName);
        if (function != null) yield return function;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return arguments;
    }

    protected override void Validate(IValidationContext context)
    {
        var function = context.ComponentNodes.GetFunction(FunctionName);
        if (function == null)
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{FunctionName}'");
            return;
        }

        if (arguments.Count > 1)
        {
            context.Logger.Fail(Reference, $"Invalid function argument: '{FunctionName}'. Should be 0 or 1.");
            return;
        }

        if (arguments.Count == 0)
        {
            FillParametersFunction.GetMapping(Reference, context, function.GetParametersType(),
                mappingParameters);
            ExtractResultsFunction.GetMapping(Reference, context, function.GetResultsType(), mappingResults);

            FunctionParametersType = function.GetParametersType();
            FunctionResultsType = function.GetResultsType();

            return;
        }

        var argumentType = arguments[0].DeriveType(context);
        var parametersType = function.GetParametersType();

        if (argumentType == null || !argumentType.Equals(parametersType))
            context.Logger.Fail(Reference, $"Invalid function argument: '{FunctionName}'. " +
                                           "Argument should be of type function parameters. Use new(Function) of fill(Function) to create an variable of the function result type.");

        VariableName = (arguments[0] as IdentifierExpression)?.Identifier;
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        var function = context.ComponentNodes.GetFunction(FunctionName);
        return function?.GetResultsType();
    }


    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return base.UsedVariables()
            .Union(mappingParameters.Select(map => map.ToUsedVariable(VariableAccess.Read))
            .Union(mappingResults.Select(map => map.ToUsedVariable(VariableAccess.Write))));
    }
}