using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public class VariableDefinition : Node, IHasNodeDependencies
{
    public Expression DefaultExpression { get; }
    public VariableSource Source { get; }
    public TypeDeclaration TypeDeclaration { get; }
    public string Name { get; }

    public VariableDefinitionState State { get; private set; }

    public VariableDefinitionState StateRequired
    {
        get
        {
            if (State == null) throw new InvalidOperationException("State not set.");
            return State;
        }
    }

    private VariableDefinition(string name,  TypeDeclaration type,
        VariableSource source, NodeReference parentReference,
        SourceReference reference, Expression defaultExpression = null) :
        base(parentReference, reference)
    {
        TypeDeclaration = Assert.NotNull(type, nameof(type));
        Name = Assert.NotNull(name, nameof(name));

        DefaultExpression = defaultExpression;
        Source = source;
    }

    public IEnumerable<IComponentNode> GetDependencies(IComponentNodeList componentNodes)
    {
        return TypeDeclaration is IHasNodeDependencies hasNodeDependencies
            ? hasNodeDependencies.GetDependencies(componentNodes)
            : Array.Empty<IComponentNode>();
    }

    public static VariableDefinition Parse(VariableSource source, IParseLineContext context, NodeReference parentReference)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var result = context.ValidateTokens<VariableDefinition>()
            .CountMinimum(2)
            .StringLiteral(1)
            .IsValid;

        if (!result) return null;

        if (!tokens.IsTokenType<StringLiteralToken>(0) && !tokens.IsTokenType<MemberAccessToken>(0))
        {
            context.Logger.Fail(tokens.Reference(0, 1), "Unexpected token.");
            return null;
        }

        var definitionReference = new NodeReference();
        var name = tokens.TokenValue(1);

        var defaultValue = ParseDefaultExpression(context, tokens, definitionReference, line);
        if (!defaultValue.IsSuccess) return null;

        var typeToken = tokens.TokenValue(0);
        var typeDeclaration = TypeDeclarationParser.Parse(typeToken, definitionReference, tokens.Reference(0, 1));
        if (typeDeclaration == null) return null;

        var variableDefinition = new VariableDefinition(name, typeDeclaration, source, parentReference, tokens.AllReference(), defaultValue.Result);
        definitionReference.SetNode(variableDefinition);
        return variableDefinition;
    }

    private static ParseExpressionResult ParseDefaultExpression(IParseLineContext context, TokenList tokens,
        NodeReference definitionReference, Line line)
    {
        if (tokens.Length <= 2)
        {
            return ParseExpressionResult.Success(null);
        }

        if (tokens.Token<OperatorToken>(2).Type != OperatorType.Assignment)
        {
            context.Logger.Fail(tokens.Reference(2, 1), "Invalid variable declaration token. Expected '='.");
            return ParseExpressionResult.Invalid<VariableDefinition>("failed");
        }

        if (tokens.Length != 4)
        {
            context.Logger.Fail(tokens.AllReference(),
                "Invalid variable declaration. Expected literal token.");
            return ParseExpressionResult.Invalid<VariableDefinition>("failed");
        }

        var defaultValue = context.ExpressionFactory.Parse(definitionReference, tokens.TokensFrom(3), line);
        return context.Failed(defaultValue, tokens.Reference(3))
            ? ParseExpressionResult.Invalid<VariableDefinition>("failed")
            : defaultValue;
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (DefaultExpression != null) yield return DefaultExpression;
        yield return TypeDeclaration;
    }

    protected override void Validate(IValidationContext context)
    {
        State = new VariableDefinitionState(TypeDeclaration.Type);

        context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, State.Type, Source);

        context.ValidateTypeAndDefault(Reference, TypeDeclaration, DefaultExpression);
    }

    public override Symbol GetSymbol()
    {
        var kind = Source == VariableSource.Parameters ? SymbolKind.ParameterVariable : SymbolKind.ResultVariable;
        var label = Label();
        return new Symbol(Reference, label, string.Empty, kind);
    }

    private string Label()
    {
        var prefix = GetPrefix();
        return $"{prefix}: {TypeDeclaration.Label()} {Name}";
    }

    private string GetPrefix()
    {
        return Source switch
        {
            VariableSource.Parameters => "parameter",
            VariableSource.Results => "result",
            VariableSource.Code => "variable",
            VariableSource.Type => "type",
            _ => throw new InvalidOperationException("Invalid source: " + Source)
        };
    }

    public override string ToString() => Label();
}
