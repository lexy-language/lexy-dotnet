using System;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Symbols;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language;

public class VariableReference
{
    public SourceReference Reference { get; }
    public IdentifierPath Path { get; }
    public VariableSource Source { get; }
    public Type ComponentType { get; }
    public Type Type { get; }

    public VariableReference(SourceReference reference, IdentifierPath path, Type componentType,
        Type type, VariableSource source)
    {
        Reference = reference;
        Path = path;
        ComponentType = componentType;
        Type = type;
        Source = source;
    }

    public Symbol GetSymbol()
    {
        return Source switch
        {
            VariableSource.Parameters => new Symbol(Reference, $"parameter: {Type} {Path}", string.Empty, SymbolKind.ParameterVariable),
            VariableSource.Results => new Symbol(Reference, $"result: {Type} {Path}", string.Empty, SymbolKind.ResultVariable),
            VariableSource.Code => new Symbol(Reference, $"variable: {Type} {Path}", string.Empty, SymbolKind.Variable),
            VariableSource.Type => TypeSymbol(),
            _ => throw new ArgumentOutOfRangeException(nameof(Source), Source.ToString())
        };
    }

    private Symbol TypeSymbol()
    {
        if (Type is EnumType)
        {
            return new Symbol(Reference, $"enum member: {Path}", string.Empty, SymbolKind.EnumMember);
        }
        return Type is GeneratedType
            ? new Symbol(Reference, $"type: {Path}", string.Empty, SymbolKind.GeneratedType)
            : new Symbol(Reference, $"variable: {Type} {Path}", string.Empty, SymbolKind.Variable);
    }

    public override string ToString()
    {
        return GetSymbol().ToString();
    }
}
