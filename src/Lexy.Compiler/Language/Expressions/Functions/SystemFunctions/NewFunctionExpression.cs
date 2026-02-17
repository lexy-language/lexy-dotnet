using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class NewFunctionExpression : FunctionCallExpression, IHasNodeDependencies
{
    public const string FunctionName = "new";

    protected string FunctionHelp => $"{Name} expects 1 argument new(Function.Parameters)";

    public MemberAccessToken TypeToken { get; }

    public Expression ValueExpression { get; }

    public NewFunctionState State { get; private set; }

    public NewFunctionState StateRequired
    {
        get
        {
            if (State == null) throw new InvalidOperationException("State not set.");
            return State;
        }
    }

    public override string Name => FunctionName;

    private NewFunctionExpression(Expression valueExpression, NodeReference parentReference, ExpressionSource source)
        : base(parentReference, source)
    {
        ValueExpression = Assert.NotNull(valueExpression, nameof(valueExpression));
        TypeToken = (valueExpression as MemberAccessExpression)?.MemberAccessToken;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        if (State?.Type?.Node != null)
        {
            yield return State.Type.Node;
        }
    }

    public static FunctionCallExpression Create(Expression expression, NodeReference parent, ExpressionSource source)
    {
        return new NewFunctionExpression(expression, parent, source);
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
                $"Invalid argument 1. 'Value' should be of type 'GeneratedType' but is '{valueType?.GetType()}'. {FunctionHelp}");
            return;
        }

        State = new NewFunctionState(generatedType);
    }

    public override Type DeriveType(IValidationContext context)
    {
        var nodeType = context.ComponentNodes.GetType(TypeToken.Parent);
        return nodeType?.MemberType(TypeToken.Member) as GeneratedType;
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, FunctionName, FunctionHelp, SymbolKind.SystemFunction);
    }
}
