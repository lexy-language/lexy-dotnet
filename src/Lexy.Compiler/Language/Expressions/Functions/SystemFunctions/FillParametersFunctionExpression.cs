using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class FillParametersFunctionExpression : FunctionCallExpression, IHasNodeDependencies
{
    public const string FunctionName = "fill";

    private VariablesMapping mapping;

    private string FunctionHelp => $"{Name} expects 1 argument fill(Function.Parameters)";

    public MemberAccessLiteralToken TypeLiteralToken { get; }

    public Expression ValueExpression { get; }

    public GeneratedType Type { get; private set; }

    public IEnumerable<Mapping> Mapping => mapping;

    public override string Name => FunctionName;

    private FillParametersFunctionExpression(Expression valueExpression, ExpressionSource source)
        : base(source)
    {
        ValueExpression = Assert.NotNull(valueExpression, nameof(valueExpression));
        TypeLiteralToken = (valueExpression as MemberAccessExpression)?.MemberAccessLiteralToken;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        if (TypeLiteralToken == null) yield break;

        var componentNode = componentNodes.GetNode(TypeLiteralToken.ToString());
        if (componentNode != null) yield return componentNode;
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new FillParametersFunctionExpression(expression, source);
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

        mapping = GetMapping(Reference, context, generatedType);
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
        var function = context.ComponentNodes.GetFunction(TypeLiteralToken.Parent);
        if (function == null) return null;

        return TypeLiteralToken.Member switch
        {
            Function.ParameterName => function.GetParametersType(),
            Function.ResultsName => function.GetResultsType(),
            _ => null
        };
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return base.UsedVariables()
            .Union(mapping.Select(map => map.ToUsedVariable(VariableAccess.Read)));
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, FunctionName, FunctionHelp, SymbolKind.SystemFunction);
    }
}
