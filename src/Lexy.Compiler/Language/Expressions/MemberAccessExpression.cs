using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class MemberAccessExpression : Expression, IHasNodeDependencies, IHasVariableReference
{
    public MemberAccessLiteral MemberAccessLiteral { get; }

    public VariablePath VariablePath { get; private set; }
    public VariableReference Variable { get; private set; }

    private MemberAccessExpression(VariablePath variablePath, MemberAccessLiteral literal, ExpressionSource source,
        SourceReference reference) : base(source, reference)
    {
        MemberAccessLiteral = literal ?? throw new ArgumentNullException(nameof(literal));
        VariablePath = variablePath;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodeList)
    {
        var componentNode = componentNodeList.GetNode(MemberAccessLiteral.Parent);
        if (componentNode != null) yield return componentNode;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<MemberAccessExpression>("Invalid expression.");

        var literal = tokens.Token<MemberAccessLiteral>(0);
        var variable = new VariablePath(literal.Parts);

        var reference = source.CreateReference();

        var accessExpression = new MemberAccessExpression(variable, literal, source, reference);
        return ParseExpressionResult.Success(accessExpression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1
               && tokens.IsTokenType<MemberAccessLiteral>(0);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        CreateVariableReference(context);
    }

    private void CreateVariableReference(IValidationContext context)
    {
        Variable = context.VariableContext.CreateVariableReference(Reference, VariablePath, context);
        if (Variable == null)
        {
            context.Logger.Fail(Reference, $"Invalid identifier: '{VariablePath.FullPath()}'");
        }
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return MemberAccessLiteral.DeriveType(context);
    }
}