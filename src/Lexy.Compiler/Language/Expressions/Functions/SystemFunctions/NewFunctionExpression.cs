using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class NewFunctionExpression : FunctionCallExpression, IHasNodeDependencies, INodeWithName
{
    public const string FunctionName = "new";

    protected string FunctionHelp => $"{Name} expects 1 argument new(Function.Parameters)";

    public MemberAccessLiteralToken TypeLiteralToken { get; }

    public Expression ValueExpression { get; }

    public GeneratedType Type { get; private set; }

    public override string Name => FunctionName;

    private NewFunctionExpression(Expression valueExpression, ExpressionSource source)
        : base(source)
    {
        ValueExpression = Assert.NotNull(valueExpression, nameof(valueExpression));
        TypeLiteralToken = (valueExpression as MemberAccessExpression)?.MemberAccessLiteralToken;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        if (Type?.Node != null)
        {
            yield return Type.Node;
        }
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new NewFunctionExpression(expression, source);
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

        Type = generatedType;
    }

    public override Type DeriveType(IValidationContext context)
    {
        var nodeType = context.ComponentNodes.GetType(TypeLiteralToken.Parent);
        return nodeType?.MemberType(TypeLiteralToken.Member) as GeneratedType;
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, FunctionName, FunctionHelp, SymbolKind.SystemFunction);
    }
}
