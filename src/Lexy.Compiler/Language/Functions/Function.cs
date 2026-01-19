using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Functions;

public class Function : ComponentNode, IHasNodeDependencies, INestedNode, INodeWithType
{
    public const string ParameterName = "Parameters";
    public const string ResultsName = "Results";

    public FunctionParameters Parameters { get; }
    public FunctionResults Results { get; }
    public FunctionCode Code { get; }

    public override string Name { get; }

    public bool Nested { get; }

    private Function(string name, bool nested, SourceReference reference, IExpressionFactory factory) : base(reference)
    {
        Nested = nested;
        Parameters = new FunctionParameters(reference);
        Results = new FunctionResults(reference);
        Code = new FunctionCode(reference, factory);
        Name = name;
    }

    public Type CreateType()
    {
        return new FunctionType(this);
    }

    public GeneratedType GetParametersType()
    {
        var members = Parameters.Variables
            .Select(parameter => new ObjectVariable(parameter.Name, parameter.TypeDeclaration.Type))
            .ToList();

        return new GeneratedType(Name, this, GeneratedTypeSource.FunctionParameters, members);
    }

    public GeneratedType GetResultsType()
    {
        var members = Results.Variables
            .Select(parameter => new ObjectVariable(parameter.Name, parameter.TypeDeclaration.Type))
            .ToList();

        return new GeneratedType(Name, this, GeneratedTypeSource.FunctionResults, members);
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var result = new List<IComponentNode>();
        AddObjectTypes(componentNodes, Parameters.Variables, result);
        AddObjectTypes(componentNodes, Results.Variables, result);
        return result;
    }

    internal static Function Create(string name, bool nested, SourceReference reference, IExpressionFactory factory)
    {
        return new Function(name, nested, reference, factory);
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
            if (parameter.TypeDeclaration is not ObjectTypeDeclaration objectType) continue;

            var dependency = objectType.GetNode(componentNodes);
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
        yield return Parameters;
        yield return Results;

        yield return Code;
    }

    protected override void Validate(IValidationContext context)
    {
        if (string.IsNullOrEmpty(Name))
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{Name}'. Name should not be empty.");
        }
        else if (!SyntaxFacts.IsValidIdentifier(Name))
        {
            context.Logger.Fail(Reference, $"Invalid function name: '{Name}'.");
        }
    }

    public ValidateFunctionArgumentsResult ValidateArguments(IValidationContext context,
        IReadOnlyList<Expression> arguments, SourceReference reference)
    {
        return HasSpreadArgument(arguments)
            ? ValidateAutoMap()
            : ValidateWithArguments(context, arguments, reference);
    }

    private ValidateFunctionArgumentsResult ValidateAutoMap()
    {
        var parametersType = GetParametersType();
        var resultsType = GetResultsType();

        return ValidateFunctionArgumentsAutoMapResult.SuccessAutoMap(parametersType, resultsType);
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
        var parameters = GetParametersTypes();
        var resultsType = GetResultsType();
        return new FunctionSignature(parameters, resultsType);
    }

    private List<Type> GetParametersTypes()
    {
        return Parameters.Variables.Select(parameter => parameter.Type).ToList();
    }

    private IReadOnlyList<Type> GetArgumentTypes(IReadOnlyList<Expression> arguments, IValidationContext context)
    {
        return HasSpreadArgument(arguments)
            ? new[] { GetResultsType() }
            : arguments.Select(argument => argument.DeriveType(context)).ToArray();
    }

    private static bool HasSpreadArgument(IReadOnlyList<Expression> arguments)
    {
        return arguments.Count == 1 && arguments[0] is SpreadExpression;
    }
}
