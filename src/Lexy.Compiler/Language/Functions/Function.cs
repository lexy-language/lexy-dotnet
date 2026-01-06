using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        AddObjectTypes(componentNodes, Parameters.Variables, result);
        AddObjectTypes(componentNodes, Results.Variables, result);
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

    private static void AddObjectTypes(IComponentNodeList componentNodes, IReadOnlyList<VariableDefinition> variableDefinitions,
        List<IComponentNode> result)
    {
        foreach (var parameter in variableDefinitions)
        {
            if (parameter.Type is not ObjectVariableTypeDeclaration complexVariableType) continue;

            var dependency = complexVariableType.GetNode(componentNodes);
            if (dependency != null)
            {
                result.Add(dependency);
            }
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
            .Select(parameter => new ObjectTypeVariable(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new GeneratedType(Name.Value, this, GeneratedTypeSource.FunctionParameters, members);
    }

    public VariableType GetResultsType()
    {
        var members = Results.Variables
            .Select(parameter => new ObjectTypeVariable(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new GeneratedType(Name.Value, this, GeneratedTypeSource.FunctionResults, members);
    }

    public ValidateFunctionArgumentsResult ValidateArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments, SourceReference reference)
    {
        return arguments.Count == 0
            ? ValidateNoArgumentCall()
            : ValidateWithArguments(context, arguments, reference);
    }

    private ValidateFunctionArgumentsResult ValidateNoArgumentCall()
    {
        return ValidateFunctionArgumentsAutoMapResult.SuccessAutoMap(GetParametersType(), GetResultsType());
    }

    private ValidateFunctionArgumentsResult ValidateWithArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments, SourceReference reference)
    {
        var argumentTypes = GetArgumentTypes(arguments, context);
        var overloads = GetFunctions();

        foreach (var overload in overloads)
        {
            if (overload.Matches(argumentTypes))
            {
                return ValidateFunctionArgumentsCallFunctionResult.Success(overload);
            }
        }

        var error = BuildErrorMessage(overloads);
        context.Logger.Fail(reference, error);

        return ValidateFunctionArgumentsResult.Failed();
    }

    private string BuildErrorMessage(IEnumerable<FunctionSignature> overloads)
    {
        var stringBuilder = new StringBuilder($"Invalid function arguments: '{Name}'. Function overloads:\n");

        foreach (var overload in overloads)
        {
            stringBuilder.Append($"- {Name}(");
            AddParameters(overload, stringBuilder);
            stringBuilder.AppendLine(")");
        }

        return stringBuilder.ToString();
    }

    private static void AddParameters(FunctionSignature signature, StringBuilder stringBuilder)
    {
        for (var index = 0; index < signature.ParametersTypes.Count; index++)
        {
            var parametersType = signature.ParametersTypes[index];
            stringBuilder.Append(parametersType);
            if (index < signature.ParametersTypes.Count - 1)
            {
                stringBuilder.Append(", ");
            }
        }
    }

    private IEnumerable<FunctionSignature> GetFunctions()
    {
        yield return GetSingleParameterArgumentFunction();
        yield return InlineParametersArgumentsFunction();
    }

    private FunctionSignature GetSingleParameterArgumentFunction()
    {
        return new FunctionSignature(new [] {GetParametersType()}, GetResultsType());
    }

    private FunctionSignature InlineParametersArgumentsFunction()
    {
        var parameters = Parameters.Variables.Select(parameter => parameter.VariableType).ToList();
        return new FunctionSignature(parameters, GetResultsType());
    }

    private IReadOnlyList<VariableType> GetArgumentTypes(IEnumerable<Expression> arguments, IValidationContext context) =>
        arguments.Select(argument => argument.DeriveType(context)).ToArray();
}