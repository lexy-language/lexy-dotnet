using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class FillParametersFunctionExpression : FunctionCallExpression, IHasNodeDependencies
{
    public const string Name = "fill";

    private readonly IList<Mapping> mapping = new List<Mapping>();

    private string FunctionHelp => $"{Name} expects 1 argument fill(Function.Parameters)";

    public MemberAccessLiteralToken TypeLiteralToken { get; }

    public Expression ValueExpression { get; }

    public GeneratedType Type { get; private set; }

    public IEnumerable<Mapping> Mapping => mapping;

    private FillParametersFunctionExpression(Expression valueExpression, ExpressionSource source)
        : base(source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
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

        GetMapping(Reference, context, generatedType, mapping);
    }

    internal static void GetMapping(SourceReference reference, IValidationContext context, GeneratedType generatedType,
        IList<Mapping> mapping)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));

        if (generatedType == null) return;

        foreach (var member in generatedType.Members)
        {
            var variable = context.VariableContext.GetVariable(member.Name);
            if (variable == null) continue;

            if (!variable.VariableType.Equals(member.Type))
            {
                context.Logger.Fail(reference,
                    $"Invalid parameter mapping. Variable '{member.Name}' of type '{variable.VariableType}' can't be mapped to parameter '{member.Name}' of type '{member.Type}'.");
            }
            else
            {
                mapping.Add(new Mapping(member.Name, variable.VariableType, variable.VariableSource));
            }
        }

        if (mapping.Count == 0)
        {
            context.Logger.Fail(reference,
                "Invalid parameter mapping. No parameter could be mapped from variables.");
        }
    }

    public override VariableType DeriveType(IValidationContext context)
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
}