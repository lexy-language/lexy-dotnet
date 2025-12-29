using System.Collections.Generic;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class IdentifierExpression : Expression, IHasVariableReference
{
    public VariableReference Variable { get; private set; }

    public string Identifier { get; }

    private IdentifierExpression(string identifier, ExpressionSource source, SourceReference reference) : base(source,
        reference)
    {
        Identifier = identifier;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<IdentifierExpression>("Invalid expression");

        var variableName = tokens.TokenValue(0);
        var reference = source.CreateReference();

        var expression = new IdentifierExpression(variableName, source, reference);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 1
               && tokens.IsTokenType<StringLiteralToken>(0);
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
        var path = IdentifierPath.Parse(Identifier);
        Variable = context.VariableContext.CreateVariableReference(Reference, path, context);
        if (Variable == null)
        {
            context.Logger.Fail(Reference, $"Invalid identifier: '{path.FullPath()}'");
        }
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return context.VariableContext.GetVariableType(Identifier);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        if (Variable != null)
        {
            yield return VariableUsage.Read(Variable);
        }
    }
}