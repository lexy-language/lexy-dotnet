using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Declaration;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language;

public class VariableDefinition : Node, IHasNodeDependencies
{
    public Expression DefaultExpression { get; }
    public VariableSource Source { get; }
    public TypeDeclaration TypeDeclaration { get; }
    public Type Type { get; private set; }
    public string Name { get; }

    private VariableDefinition(string name, TypeDeclaration type,
        VariableSource source, SourceReference reference, Expression defaultExpression = null) : base(reference)
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

    public static VariableDefinition Parse(VariableSource source, IParseLineContext context)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var result = context.ValidateTokens<VariableDefinition>()
            .CountMinimum(2)
            .StringLiteral(1)
            .IsValid;

        if (!result) return null;

        if (!tokens.IsTokenType<StringLiteralToken>(0) && !tokens.IsTokenType<MemberAccessLiteralToken>(0))
        {
            context.Logger.Fail(tokens.Reference(0, 1), "Unexpected token.");
            return null;
        }

        var typeToken = tokens.TokenValue(0);

        var type = TypeDeclarationParser.Parse(typeToken, tokens.Reference(0, 1));
        if (type == null) return null;

        var name = tokens.TokenValue(1);
        if (tokens.Length == 2)
        {
            return new VariableDefinition(name, type, source, tokens.AllReference());
        }

        if (tokens.Token<OperatorToken>(2).Type != OperatorType.Assignment)
        {
            context.Logger.Fail(tokens.Reference(2, 1), "Invalid variable declaration token. Expected '='.");
            return null;
        }

        if (tokens.Length != 4)
        {
            context.Logger.Fail(tokens.AllReference(),
                "Invalid variable declaration. Expected literal token.");
            return null;
        }

        var defaultValue = context.ExpressionFactory.Parse(tokens.TokensFrom(3), line);
        if (context.Failed(defaultValue, tokens.Reference(3))) return null;

        return new VariableDefinition(name, type, source, tokens.AllReference(), defaultValue.Result);
    }

    public override IEnumerable<INode> GetChildren()
    {
        if (DefaultExpression != null) yield return DefaultExpression;
        yield return TypeDeclaration;
    }

    protected override void Validate(IValidationContext context)
    {
        Type = TypeDeclaration.Type;

        context.VariableContext.RegisterVariableAndVerifyUnique(Reference, Name, Type, Source);

        context.ValidateTypeAndDefault(Reference, TypeDeclaration, DefaultExpression);
    }

    public override Symbol GetSymbol()
    {
        var kind = Source == VariableSource.Parameters ? SymbolKind.ParameterVariable : SymbolKind.ResultVariable;
        var prefix = GetPrefix();
        return new Symbol(Reference, $"{prefix}: {Type} {Name}", string.Empty, kind);
    }

    private string GetPrefix()
    {
        return Source switch
        {
            VariableSource.Parameters => "parameter",
            VariableSource.Results => "result",
            VariableSource.Code => "variable",
            VariableSource.Type => "type",
            _ => "unknown"
        };
    }
}
