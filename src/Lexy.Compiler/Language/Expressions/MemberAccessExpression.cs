using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class MemberAccessExpression : Expression, IHasNodeDependencies, IHasVariableReference
{
    public MemberAccessLiteralToken MemberAccessLiteralToken { get; }

    public IdentifierPath VariablePath { get; }
    public VariableReference Variable { get; private set; }

    private MemberAccessExpression(IdentifierPath variablePath, MemberAccessLiteralToken literalToken, ExpressionSource source,
        SourceReference reference) : base(source, reference)
    {
        MemberAccessLiteralToken = literalToken ?? throw new ArgumentNullException(nameof(literalToken));
        VariablePath = variablePath;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var componentNode = componentNodes.GetNode(MemberAccessLiteralToken.Parent);
        if (componentNode != null) yield return componentNode;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<MemberAccessExpression>("Invalid expression.");

        var literal = tokens.Token<MemberAccessLiteralToken>(0);
        var variable = new IdentifierPath(literal.Parts);

        var reference = source.CreateReference();

        var accessExpression = new MemberAccessExpression(variable, literal, source, reference);
        return ParseExpressionResult.Success(accessExpression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1
            && tokens.IsTokenType<MemberAccessLiteralToken>(0);
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
        return MemberAccessLiteralToken.DeriveType(context);
    }
}