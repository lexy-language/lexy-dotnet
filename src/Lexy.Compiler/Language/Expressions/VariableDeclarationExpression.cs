using System.Collections.Generic;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class VariableDeclarationExpression : Expression
{
    private Type type;

    public TypeDeclaration TypeDeclaration { get; }
    public string Name { get; }
    public Expression Assignment { get; }

    private VariableDeclarationExpression(TypeDeclaration typeDeclaration, string variableName, Expression assignment,
        ExpressionSource source, SourceReference reference) : base(source, reference)
    {
        TypeDeclaration = Assert.NotNull(typeDeclaration, nameof(typeDeclaration));
        Name = Assert.NotNull(variableName, nameof(variableName));
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens))
            return ParseExpressionResult.Invalid<VariableDeclarationExpression>("Invalid expression.");

        var type = VariableDeclarationTypeParser.Parse(tokens.TokenValue(0), source.CreateReference());
        var name = tokens.TokenValue(1);
        var assignment = tokens.Length > 3
            ? factory.Parse(tokens.TokensFrom(3), source.Line)
            : null;
        if (assignment is { IsSuccess: false }) return assignment;

        var reference = source.CreateReference();

        var expression = new VariableDeclarationExpression(type, name, assignment?.Result, source, reference);

        return ParseExpressionResult.Success(expression);
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
               && tokens.IsTokenType<MemberAccessLiteralToken>(0)
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
        if (Assignment != null) yield return Assignment;
    }

    protected override void Validate(IValidationContext context)
    {
        var assignmentType = Assignment?.DeriveType(context);
        if (Assignment != null && assignmentType == null)
        {
            context.Logger.Fail(Reference, "Invalid expression. Could not derive type.");
        }

        var variableType = GetVariableType(context, assignmentType);
        if (variableType == null)
        {
            context.Logger.Fail(Reference, $"Invalid variable type '{TypeDeclaration}'");
        }

        context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, variableType, VariableSource.Code);
    }

    private Type GetVariableType(IValidationContext context, Type assignmentType)
    {
        if (TypeDeclaration is ImplicitTypeDeclaration implicitVariableType)
        {
            implicitVariableType.Define(assignmentType);
            return assignmentType;
        }

        type = TypeDeclaration.Type;
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
        yield return new VariableUsage(IdentifierPath.Parse(Name), null, type, VariableSource.Code, VariableAccess.Write);

        if (Assignment == null) yield break;

        var readVariables = Assignment.GetReadVariableUsage();
        foreach (var readVariable in readVariables)
        {
            yield return readVariable;
        }
    }
}
