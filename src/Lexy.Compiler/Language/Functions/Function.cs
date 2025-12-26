using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
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

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodeList)
    {
        var result = new List<IComponentNode>();
        AddEnumTypes(componentNodeList, Parameters.Variables, result);
        AddEnumTypes(componentNodeList, Results.Variables, result);
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

    private IParsableNode InvalidKeyword(string name, IParseLineContext parserContext)
    {
        parserContext.Logger.Fail(Reference, $"Invalid keyword '{name}'.");
        return this;
    }

    private static void AddEnumTypes(IComponentNodeList componentNodeList, IReadOnlyList<VariableDefinition> variableDefinitions,
        List<IComponentNode> result)
    {
        foreach (var parameter in variableDefinitions)
        {
            if (parameter.Type is not CustomVariableDeclarationType enumVariableType) continue;

            var dependency = componentNodeList.GetEnum(enumVariableType.Type);
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

    public ComplexType GetParametersType()
    {
        var members = Parameters.Variables
            .Select(parameter => new ComplexTypeMember(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new ComplexType(Name.Value, this, ComplexTypeSource.FunctionParameters, members);
    }

    public ComplexType GetResultsType()
    {
        var members = Results.Variables
            .Select(parameter => new ComplexTypeMember(parameter.Name, parameter.Type.VariableType))
            .ToList();

        return new ComplexType(Name.Value, this, ComplexTypeSource.FunctionResults, members);
    }
}