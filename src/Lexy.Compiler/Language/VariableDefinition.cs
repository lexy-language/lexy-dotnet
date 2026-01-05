using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Expressions.Functions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Language.VariableTypes.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language;

public class VariableDefinition : Node, IHasNodeDependencies
{
    public Expression DefaultExpression { get; }
    public VariableSource Source { get; }
    public VariableTypeDeclaration Type { get; }
    public VariableType VariableType { get; private set; }
    public string Name { get; }

    private VariableDefinition(string name, VariableTypeDeclaration type,
        VariableSource source, SourceReference reference, Expression defaultExpression = null) : base(reference)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name ?? throw new ArgumentNullException(nameof(name));

        DefaultExpression = defaultExpression;
        Source = source;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        return Type is IHasNodeDependencies hasNodeDependencies
            ? hasNodeDependencies.GetDependencies(componentNodes)
            : Array.Empty<IComponentNode>();
    }

    public static VariableDefinition Parse(VariableSource source, IParseLineContext context)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var result = context.ValidateTokens<VariableDefinition>()
            .CountMinimum(2)
            .StringLiteral(1)
            .IsValid;

        if (!result) return null;

        if (!tokens.IsTokenType<StringLiteralToken>(0) && !tokens.IsTokenType<MemberAccessLiteralToken>(0)) {
            context.Logger.Fail(line.TokenReference(0), "Unexpected token.");
            return null;
        }

        var name = tokens.TokenValue(1);
        var type = tokens.TokenValue(0);

        var variableType = VariableDeclarationTypeParser.Parse(type, line.TokenReference(0));
        if (variableType == null) return null;

        if (tokens.Length == 2) return new VariableDefinition(name, variableType, source, line.LineStartReference());

        if (tokens.Token<OperatorToken>(2).Type != OperatorType.Assignment)
        {
            context.Logger.Fail(line.TokenReference(2), "Invalid variable declaration token. Expected '='.");
            return null;
        }

        if (tokens.Length != 4)
        {
            context.Logger.Fail(line.LineEndReference(),
                "Invalid variable declaration. Expected literal token.");
            return null;
        }

        var defaultValue = context.ExpressionFactory.Parse(tokens.TokensFrom(3), line);
        if (context.Failed(defaultValue, line.TokenReference(3))) return null;

        return new VariableDefinition(name, variableType, source, line.LineStartReference(), defaultValue.Result);
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (DefaultExpression != null) yield return DefaultExpression;
        yield return Type;
    }

    protected override void Validate(IValidationContext context)
    {
        VariableType = Type.VariableType;

        context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, VariableType, Source);

        context.ValidateTypeAndDefault(Reference, Type, DefaultExpression);
    }
}