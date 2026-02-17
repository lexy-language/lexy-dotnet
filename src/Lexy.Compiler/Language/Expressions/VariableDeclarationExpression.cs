using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class VariableDeclarationExpression : Expression
{
    public TypeDeclaration TypeDeclaration { get; }
    public string Name { get; }
    public Expression Assignment { get; }
    public VariableNameExpression NameExpression { get; }

    public VariableDeclarationState State { get; private set; }

    public VariableDeclarationState StateRequired
    {
        get
        {
            if (State == null) throw new InvalidOperationException("State not set.");
            return State;
        }
    }

    private VariableDeclarationExpression(TypeDeclaration typeDeclaration, VariableNameExpression nameExpression, Expression assignment,
        ExpressionSource source, NodeReference parentReference, SourceReference reference) : base(source, parentReference, reference)
    {
        TypeDeclaration = Assert.NotNull(typeDeclaration, nameof(typeDeclaration));
        NameExpression = Assert.NotNull(nameExpression, nameof(nameExpression));
        Name = nameExpression.Name;
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens))
        {
            return ParseExpressionResult.Invalid<VariableDeclarationExpression>("Invalid expression.");
        }

        var expressionReference = new NodeReference();
        var type = TypeDeclarationParser.Parse(tokens.TokenValue(0), expressionReference, tokens.Reference(0, 1));
        var assignment = tokens.Length > 3
            ? factory.Parse(expressionReference, tokens.TokensFrom(3), source.Line)
            : null;
        if (assignment is { IsSuccess: false }) return assignment;

        var name = GetNameExpression(expressionReference, tokens);
        if (!name.IsSuccess)
        {
            return ParseExpressionResult.Invalid<VariableDeclarationExpression>("Invalid expression.");
        }

        var reference = source.CreateReference();

        var expression = new VariableDeclarationExpression(type, name.Result, assignment?.Result, source, parentReference, reference);
        expressionReference.SetNode(expression);

        return ParseExpressionResult.Success(expression);
    }

    private static ParseVariableNameExpressionResult GetNameExpression(NodeReference expressionReference, TokenList tokens)
    {
        var nameToken = tokens[1];
        var nameTokens = new TokenList(tokens.Line, nameToken);
        var expressionSource = new ExpressionSource(tokens.Line, nameTokens);
        return VariableNameExpression.Parse(expressionSource, expressionReference, SymbolKind.Variable);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length == 2
               && tokens.IsKeyword(0, Keywords.ImplicitVariableDeclaration)
               && tokens.IsTokenType<StringLiteralToken>(1)
            || tokens.Length == 2
               && tokens.IsTokenType<StringLiteralToken>(0)
               && tokens.IsTokenType<StringLiteralToken>(1)
            || tokens.Length == 2
               && tokens.IsTokenType<MemberAccessToken>(0)
               && tokens.IsTokenType<StringLiteralToken>(1)
            || tokens.Length >= 4
               && tokens.IsKeyword(0, Keywords.ImplicitVariableDeclaration)
               && tokens.IsTokenType<StringLiteralToken>(1)
               && tokens.IsOperatorToken(2, OperatorType.Assignment)
            || tokens.Length >= 4
               && tokens.IsTokenType<StringLiteralToken>(0)
               && tokens.IsTokenType<StringLiteralToken>(1)
               && tokens.IsOperatorToken(2, OperatorType.Assignment);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return TypeDeclaration;
        yield return NameExpression;
        if (Assignment != null) yield return Assignment;
    }

    protected override void Validate(IValidationContext context)
    {
        var assignmentType = Assignment?.DeriveType(context);
        if (Assignment != null && assignmentType == null)
        {
            context.Logger.Fail(Reference, "Invalid expression. Could not derive type.");
        }

        var type = GetType(context, assignmentType);
        if (type == null)
        {
            context.Logger.Fail(Reference, $"Invalid variable type '{TypeDeclaration.Label()}'");
        }

        context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, type, VariableSource.Code);

        State = new VariableDeclarationState(type);
    }

    private Type GetType(IValidationContext context, Type assignmentType)
    {
        if (TypeDeclaration is ImplicitTypeDeclaration implicitType)
        {
            implicitType.Define(assignmentType);
            return assignmentType;
        }

        var type = TypeDeclaration.Type;
        if (Assignment != null && (assignmentType == null || !assignmentType.Equals(type)))
        {
            context.Logger.Fail(Reference, "Invalid expression. Literal or enum value expression expected.");
        }

        return type;
    }

    public override Type DeriveType(IValidationContext context)
    {
        return null;
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        yield return new VariableUsage(Reference, IdentifierPath.Parse(Name), null, StateRequired.Type, VariableSource.Code, VariableAccess.Write);

        if (Assignment == null) yield break;

        var readVariables = Assignment.GetReadVariableUsage();
        foreach (var readVariable in readVariables)
        {
            yield return readVariable;
        }
    }

    public override Symbol GetSymbol() => null;
}
