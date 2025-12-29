using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;

public class NewFunction : FunctionCallExpression, IHasNodeDependencies
{
    public const string Name = "new";

    protected string FunctionHelp => $"{Name} expects 1 argument new(Function.Parameters)";

    public MemberAccessLiteralToken TypeLiteralToken { get; }

    public Expression ValueExpression { get; }

    public ComplexType Type { get; private set; }

    private NewFunction(Expression valueExpression, ExpressionSource source)
        : base(source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        TypeLiteralToken = (valueExpression as MemberAccessExpression)?.MemberAccessLiteralToken;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        if (Type != null) yield return componentNodes.GetNode(Type.Name);
    }

    public static FunctionCallExpression Create(ExpressionSource source, Expression expression)
    {
        return new NewFunction(expression, source);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        var valueType = ValueExpression.DeriveType(context);
        if (valueType is not ComplexType complexType)
        {
            context.Logger.Fail(Reference,
                $"Invalid argument 1. 'Value' should be of type 'ComplexType' but is '{valueType?.GetType()}'. {FunctionHelp}");
            return;
        }

        Type = complexType;
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        var nodeType = context.ComponentNodes.GetType(TypeLiteralToken.Parent);
        return nodeType?.MemberType(TypeLiteralToken.Member, context.ComponentNodes) as ComplexType;
    }
}