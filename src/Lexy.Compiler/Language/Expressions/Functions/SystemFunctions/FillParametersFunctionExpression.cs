using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class FillParametersFunctionExpression : FunctionCallExpression, IHasNodeDependencies
{
    public const string FunctionName = "fill";

    private string FunctionHelp => $"{Name} expects 1 argument fill(Function.Parameters)";

    public MemberAccessToken TypeToken { get; }

    public Expression ValueExpression { get; }

    public GeneratedType Type { get; private set; }

    public override string Name => FunctionName;

    public FillParametersFunctionState State { get; private set; }

    private FillParametersFunctionExpression(Expression valueExpression, NodeReference parentReference, ExpressionSource source)
        : base(parentReference, source)
    {
        ValueExpression = Assert.NotNull(valueExpression, nameof(valueExpression));
        TypeToken = (valueExpression as MemberAccessExpression)?.MemberAccessToken;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        if (TypeToken == null) yield break;

        var componentNode = componentNodes.GetNode(TypeToken.ToString());
        if (componentNode != null) yield return componentNode;
    }

    public static FunctionCallExpression Create(Expression expression, NodeReference parent, ExpressionSource source)
    {
        return new FillParametersFunctionExpression(expression, parent, source);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        var valueType = ValueExpression.DeriveType(context);
        if (valueType is not GeneratedType generatedType)
        {
            context.Logger.Fail(Reference,
                $"Invalid argument 1. 'Value' should be of type 'GeneratedType' but is '{valueType}'. {FunctionHelp}");
            return;
        }

        Type = generatedType;

        var mapping = GetMapping(Reference, context, generatedType);
        State = new FillParametersFunctionState(mapping);
    }

    internal static VariablesMapping GetMapping(SourceReference reference, IValidationContext context, GeneratedType generatedType)
    {
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(context, nameof(context));

        if (generatedType == null) return null;

        var mapping = new List<Mapping>();
        foreach (var member in generatedType.Members)
        {
            var variable = context.VariableContext.GetVariable(member.Name);
            if (variable == null) continue;

            if (!variable.Type.Equals(member.Type))
            {
                context.Logger.Fail(reference,
                    $"Invalid parameter mapping. Variable '{member.Name}' of type '{variable.Type}' can't be mapped to parameter '{member.Name}' of type '{member.Type}'.");
            }
            else
            {
                mapping.Add(new Mapping(reference, member.Name, variable.Type, variable.VariableSource));
            }
        }

        if (mapping.Count == 0)
        {
            context.Logger.Fail(reference,
                "Invalid parameter mapping. No parameter could be mapped from variables.");
        }

        return new VariablesMapping(generatedType, mapping);
    }

    public override Type DeriveType(IValidationContext context)
    {
        var function = context.ComponentNodes.GetFunction(TypeToken.Parent);
        if (function == null) return null;

        return TypeToken.Member switch
        {
            Function.ParameterName => function.GetParametersType(),
            Function.ResultsName => function.GetResultsType(),
            _ => null
        };
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return base.UsedVariables()
            .Union(State.Mapping.Select(map => map.ToUsedVariable(VariableAccess.Read)));
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, FunctionName, FunctionHelp, SymbolKind.SystemFunction);
    }
}
