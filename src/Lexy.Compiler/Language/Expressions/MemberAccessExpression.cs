using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class MemberAccessExpression : Expression, IHasNodeDependencies, IHasVariableReference
{
    public MemberAccessLiteralToken MemberAccessLiteralToken { get; }

    public IdentifierPath IdentifierPath { get; }
    public VariableReference Variable { get; private set; }

    private MemberAccessExpression(IdentifierPath identifierPath, MemberAccessLiteralToken literalToken, ExpressionSource source,
        SourceReference reference) : base(source, reference)
    {
        MemberAccessLiteralToken = Assert.NotNull(literalToken, nameof(literalToken));
        IdentifierPath = identifierPath;
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
        Variable = context.VariableContext.CreateVariableReference(Reference, IdentifierPath);
        if (Variable == null)
        {
            context.Logger.Fail(Reference, $"Invalid identifier: '{IdentifierPath.FullPath()}'");
        }
    }

    public override Type DeriveType(IValidationContext context)
    {
        return MemberAccessLiteralToken.DeriveType(context);
    }

    public override Symbol GetSymbol()
    {
        return Variable != null
             ? Variable.GetSymbol()
             : new Symbol(Reference, MemberAccessLiteralToken.ToString(), string.Empty, SymbolKind.Variable);
    }
}
