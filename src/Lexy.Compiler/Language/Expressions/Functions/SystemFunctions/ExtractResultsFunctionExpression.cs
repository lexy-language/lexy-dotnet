using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class ExtractResultsFunctionExpression : FunctionCallExpression
{
    public const string FunctionName = "extract";

    private string FunctionHelp => $"{Name} expects 1 argument. extract(variable)";

    public string FunctionResultVariable { get; }
    public Expression ValueExpression { get; }

    public override string Name => FunctionName;

    public ExtractResultsFunctionState State { get; private set; }

    private ExtractResultsFunctionExpression(Expression valueExpression, NodeReference parentReference, ExpressionSource source)
        : base(parentReference, source)
    {
        ValueExpression = valueExpression;
        FunctionResultVariable = (valueExpression as IdentifierExpression)?.Identifier;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        if (FunctionResultVariable == null)
        {
            context.Logger.Fail(Reference, $"Invalid variable argument. {FunctionHelp}");
            return;
        }

        var type = context.VariableContext.GetType(FunctionResultVariable);
        if (type == null)
        {
            context.Logger.Fail(Reference, $"Unknown variable: '{FunctionResultVariable}'. {FunctionHelp}");
            return;
        }

        var generatedType = type as GeneratedType;
        if (generatedType == null)
        {
            context.Logger.Fail(Reference,
                $"Invalid variable type: '{FunctionResultVariable}'. " +
                "Should be Function Results. " +
                $"Use new(Function.Results) or fill(Function.Results) to create new function results. {FunctionHelp}");
        }

        var mapping = GetMapping(Reference, context, generatedType);
        State = new ExtractResultsFunctionState(mapping);
    }

    internal static VariablesMapping GetMapping(SourceReference reference, IValidationContext context, GeneratedType generatedType)
    {
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(context, nameof(context));

        if (generatedType == null) return null;

        var mapping = new List<Mapping>();

        foreach (var member in generatedType.Members)
        {
            AddMapping(reference, context, member, mapping);
        }

        if (mapping.Count == 0)
        {
            context.Logger.Fail(reference,
                "Invalid parameter mapping. No parameter could be mapped from variables.");
        }

        return new VariablesMapping(generatedType, mapping);
    }

    private static void AddMapping(SourceReference reference, IValidationContext context, IObjectMember member,
        List<Mapping> mapping)
    {
        var variable = context.VariableContext.GetVariable(member.Name);
        if (variable == null || variable.VariableSource == VariableSource.Parameters) return;

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

    public override Type DeriveType(IValidationContext context) => new VoidType();

    public static FunctionCallExpression Create(Expression expression, NodeReference parent, ExpressionSource source)
    {
        return new ExtractResultsFunctionExpression(expression, parent, source);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return base.UsedVariables()
            .Union(State.Mapping.Select(map => map.ToUsedVariable(VariableAccess.Write)));
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, FunctionName, FunctionHelp, SymbolKind.SystemFunction);
    }
}
