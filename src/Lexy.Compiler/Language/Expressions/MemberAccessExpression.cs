using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class MemberAccessExpression : Expression, IHasNodeDependencies, IHasVariableReference
{
    public MemberAccessToken MemberAccessToken { get; }

    public IdentifierPath IdentifierPath { get; }
    public VariableReference Variable { get; private set; }

    public string Path => MemberAccessToken.Value;

    private MemberAccessExpression(IdentifierPath identifierPath, MemberAccessToken token,
        ExpressionSource source, NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        MemberAccessToken = Assert.NotNull(token, nameof(token));
        IdentifierPath = Assert.NotNull(identifierPath, nameof(identifierPath));
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        var componentNode = componentNodes.GetNode(MemberAccessToken.Parent);
        if (componentNode != null) yield return componentNode;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<MemberAccessExpression>("Invalid expression.");

        var literal = tokens.Token<MemberAccessToken>(0);
        var variable = new IdentifierPath(literal.Parts);

        var reference = source.CreateReference();

        var accessExpression = new MemberAccessExpression(variable, literal, source, parentReference, reference);
        return ParseExpressionResult.Success(accessExpression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1
            && tokens.IsTokenType<MemberAccessToken>(0);
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
        Variable = context.VariableContext.CreateVariableReference(Reference, IdentifierPath);
        if (Variable == null)
        {
            context.Logger.Fail(Reference, $"Invalid identifier: '{IdentifierPath.FullPath()}'");
        }
    }

    public override Type DeriveType(IValidationContext context)
    {
        return MemberAccessToken.DeriveType(context);
    }

    public override Symbol GetSymbol()
    {
        return Variable != null
             ? Variable.GetSymbol()
             : new Symbol(Reference, MemberAccessToken.ToString(), string.Empty, SymbolKind.Variable);
    }
}
