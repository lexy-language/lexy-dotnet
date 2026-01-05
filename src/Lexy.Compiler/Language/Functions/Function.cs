using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Functions;

public class Function : ComponentNode, IHasNodeDependencies
{
    public const string ParameterName = "Parameters";
    public const string ResultsName = "Results";

    public FunctionName Name { get; }
    public FunctionParameters Parameters { get; }
    public FunctionResults Results { get; }
    public FunctionCode Code { get; }

    public override string NodeName => Name.Value;

    private Function(string name, SourceReference reference, IExpressionFactory factory) : base(reference)
    {
        Name = new FunctionName(reference);
        Parameters = new FunctionParameters(reference);
        Results = new FunctionResults(reference);
        Code = new FunctionCode(reference, factory);

        Name.ParseName(name);
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var result = new List<IComponentNode>();
        AddEnumTypes(componentNodes, Parameters.Variables, result);
        AddEnumTypes(componentNodes, Results.Variables, result);
        return result;
    }

    internal static Function Create(string name, SourceReference reference, IExpressionFactory factory)
    {
        return new Function(name, reference, factory);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;
        if (!line.Tokens.IsTokenType<KeywordToken>(0))
        {
            return Code.Parse(context);
        }

        var name = line.Tokens.TokenValue(0);
        return name switch
        {
            Keywords.Parameters => Parameters,
            Keywords.Results => Results,
            _ => Code.Parse(context)
        };
    }

    private static void AddEnumTypes(IComponentNodeList componentNodes, IReadOnlyList<VariableDefinition> variableDefinitions,
        List<IComponentNode> result)
    {
        foreach (var parameter in variableDefinitions)
        {
            if (parameter.Type is not ComplexVariableTypeDeclaration enumVariableType) continue;

            var dependency = componentNodes.GetEnum(enumVariableType.Type);
            if (dependency != null) result.Add(dependency);
        }
    }

    public override void ValidateTree(IValidationContext context)
    {
        using (context.CreateVariableScope())
        {
            base.ValidateTree(context);
        }
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Name;

        yield return Parameters;
        yield return Results;

        yield return Code;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public GeneratedType GetParametersType()
    {
        var members = Parameters.Variables
            .Select(parameter => new GeneratedTypeMember(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new GeneratedType(Name.Value, this, GeneratedTypeSource.FunctionParameters, members);
    }

    public VariableType GetResultsType()
    {
        var members = Results.Variables
            .Select(parameter => new GeneratedTypeMember(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new GeneratedType(Name.Value, this, GeneratedTypeSource.FunctionResults, members);
    }

    public ValidateFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments)
    {
        return arguments.Count == 0
            ? ValidateNoArgumentCall()
            : ValidateWithArguments(context, arguments);
    }

    private ValidateFunctionArgumentsResult ValidateNoArgumentCall()
    {
        return ValidateFunctionArgumentsResult.SuccessAutoMap(GetParametersType(), GetResultsType());
    }

    private ValidateFunctionArgumentsResult ValidateWithArguments(IValidationContext context, IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != 1)
        {
            context.Logger.Fail(Reference, $"Invalid number of function arguments: '{Name}'. ");
            return ValidateFunctionArgumentsResult.Failed();
        }

        var argumentType = arguments[0].DeriveType(context);
        var resultsType = GetResultsType();
        var parametersType = GetParametersType();

        if (argumentType == null || !argumentType.Equals(parametersType))
        {
            context.Logger.Fail(Reference, $"Invalid function argument: '{Name}'. " +
                                           "Argument should be of type function parameters. Use new(Function) of fill(Function) to create an variable of the function result type.");

            return ValidateFunctionArgumentsResult.Failed();
        }

        return ValidateFunctionArgumentsResult.Success(resultsType);
    }
}