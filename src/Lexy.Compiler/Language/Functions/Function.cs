using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Functions;

public class Function : ComponentNode, IHasNodeDependencies, INestedNode, INodeWithType
{
    public const string ParameterName = "Parameters";
    public const string ResultsName = "Results";

    public FunctionParameters Parameters { get; private set; }
    public FunctionResults Results { get; private set; }
    public FunctionCode Code { get; }

    public bool Nested { get; }

    private Function(string name, bool nested, NodeReference parentReference, SourceReference reference) :
        base(name, parentReference, reference)
    {
        Nested = nested;
        Code = new FunctionCode(this, reference);
    }

    public Type CreateType()
    {
        return new FunctionType(this);
    }

    public GeneratedType GetParametersType()
    {
        var members = GetMembers(Parameters?.Variables);
        return new GeneratedType(Name, ParameterName, this, GeneratedTypeSource.FunctionParameters, members);
    }

    public GeneratedType GetResultsType()
    {
        var members = GetMembers(Results?.Variables);
        return new GeneratedType(Name, ResultsName, this, GeneratedTypeSource.FunctionResults, members);
    }

    private static IEnumerable<ObjectVariable> GetMembers(IReadOnlyList<VariableDefinition>  variables)
    {
        if (variables == null)
        {
            return new List<ObjectVariable>();
        }
        return variables
            .Select(parameter => new ObjectVariable(parameter.Name, parameter.TypeDeclaration.Type))
            .ToList();
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var result = new List<IComponentNode>();
        AddObjectTypes(componentNodes, Parameters?.Variables, result);
        AddObjectTypes(componentNodes, Results?.Variables, result);
        return result;
    }

    internal static Function Create(string name, bool nested, NodeReference parentReference, SourceReference reference)
    {
        return new Function(name, nested, parentReference, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;
        if (!line.Tokens.IsTokenType<KeywordToken>(0))
        {
            return ParseCode(context);
        }

        var reference = line.Tokens.AllReference();
        var name = line.Tokens.TokenValue(0);
        return name switch
        {
            Keywords.Parameters => Parameters = new FunctionParameters(this, reference),
            Keywords.Results => Results = new FunctionResults(this, reference),
            _ => ParseCode(context)
        };
    }

    private IParsableNode ParseCode(IParseLineContext context)
    {
        Code.ExpandArea(context.Line.EndPosition);
        return Code.Parse(context);
    }

    private static void AddObjectTypes(IComponentNodeList componentNodes,
        IReadOnlyList<VariableDefinition> variableDefinitions,
        ICollection<IComponentNode> result)
    {
        if (variableDefinitions == null) return;

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
        context.InNodeVariableScope(this, base.ValidateTree);
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (Parameters != null) yield return Parameters;
        if (Results != null) yield return Results;

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

        return ValidateFunctionArgumentsAutoMapResult.SuccessAutoMap(parametersType);
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
        return Parameters.Variables.Select(parameter => parameter.State.Type).ToList();
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

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, $"function: {Name}", string.Empty, SymbolKind.Function);
    }

    public override SuggestionEdit[] GetSuggestions()
    {
        return Suggestions.Edit(with => with
             .Keyword(Keywords.Parameters)
             .Keyword(Keywords.Results)
             .Keyword(Keywords.If)
             .Keyword(Keywords.Switch)
         );
    }

    public void AddParametersAndResultsForVariables(IValidationContext context)
    {
        AddVariablesForValidation(context, Parameters?.Variables, VariableSource.Parameters);
        AddVariablesForValidation(context, Results?.Variables, VariableSource.Results);
    }

    private static void AddVariablesForValidation(IValidationContext context, IReadOnlyList<VariableDefinition> definitions,
        VariableSource source)
    {
        if (definitions == null) return;

        foreach (var definition in definitions)
        {
            var type = definition.TypeDeclaration.Type;
            context.VariableContext.AddVariable(definition.Name, type, source);
        }
    }
}
