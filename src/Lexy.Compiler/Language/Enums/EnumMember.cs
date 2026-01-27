using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Compiler.Language.Enums;

public class EnumMember : Node
{
    public string Name { get; }

    public NumberLiteralToken ValueLiteral { get; }

    public int NumberValue { get; }

    private EnumMember(string name, NumberLiteralToken valueLiteral, int value, SourceReference reference) :
        base(reference)
    {
        NumberValue = value;
        Name = name;
        ValueLiteral = valueLiteral;
    }

    public static EnumMember Parse(IParseLineContext context, int lastIndex)
    {
        var valid = context.ValidateTokens<EnumMember>()
            .CountMinimum(1)
            .StringLiteral(0)
            .IsValid;

        if (!valid) return null;

        var line = context.Line;
        var tokens = line.Tokens;
        var name = tokens.TokenValue(0);

        if (tokens.Length == 1)
        {
            return new EnumMember(name, null, lastIndex + 1, tokens.Reference(0, 1));
        }

        if (tokens.Length != 3)
        {
            context.Logger.Fail(tokens.AllReference(),
                $"Invalid number of tokens: {tokens.Length}. Should be 1 or 3.");
            return null;
        }

        valid = context.ValidateTokens<EnumMember>()
            .Operator(1, OperatorType.Assignment)
            .NumberLiteral(2)
            .IsValid;
        if (!valid) return null;

        var value = tokens.Token<NumberLiteralToken>(2);
        var reference = tokens.AllReference();

        return new EnumMember(name, value, (int)value.NumberValue, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
        ValidateMemberName(context);
        ValidateMemberValues(context);
    }

    private void ValidateMemberName(IValidationContext context)
    {
        if (string.IsNullOrEmpty(Name))
        {
            context.Logger.Fail(Reference, "Enum member name should not be null or empty.");
        }
        else if (!SyntaxFacts.IsValidIdentifier(Name))
        {
            context.Logger.Fail(Reference, $"Invalid enum member name: {Name}.");
        }
    }

    private void ValidateMemberValues(IValidationContext context)
    {
        if (ValueLiteral == null) return;

        if (ValueLiteral.NumberValue < 0)
        {
            context.Logger.Fail(Reference, $"Enum member value should not be < 0: {ValueLiteral}");
        }

        if (ValueLiteral.IsDecimal())
        {
            context.Logger.Fail(Reference, $"Enum member value should not be decimal: {ValueLiteral}");
        }
    }

    public override Symbol GetSymbol()
    {
        var parentEnum = Parent as EnumDefinition;
        return ValueLiteral != null
            ? new Symbol(Reference, $"enum member: {parentEnum?.Name}.{Name} = {ValueLiteral}", string.Empty, SymbolKind.EnumMember)
            : new Symbol(Reference, $"enum member: {parentEnum?.Name}.{Name}", string.Empty, SymbolKind.EnumMember);
    }

    public override string ToString() => Name;
}
