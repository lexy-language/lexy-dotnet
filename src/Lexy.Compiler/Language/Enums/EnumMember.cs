using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Microsoft.CodeAnalysis.CSharp;

namespace Lexy.Compiler.Language.Enums;

public class EnumMember : Node
{
    public string Name { get; }

    public NumberLiteralToken ValueLiteral { get; }

    public int NumberValue { get; }

    private EnumMember(string name, NumberLiteralToken valueLiteral, int value, EnumDefinition enumDefinition, SourceReference reference) :
        base(enumDefinition, reference)
    {
        NumberValue = value;
        Name = name;
        ValueLiteral = valueLiteral;
    }

    public static EnumMember Parse(IParseLineContext context, EnumDefinition enumDefinition, int lastIndex)
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
            return new EnumMember(name, null, lastIndex + 1, enumDefinition, tokens.Reference(0, 1));
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

        return new EnumMember(name, value, (int)value.NumberValue, enumDefinition, reference);
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
            context.Logger.Fail(Reference, $"Enum member value should not be < 0: {ValueLiteral.Value}");
        }

        if (ValueLiteral.IsDecimal())
        {
            context.Logger.Fail(Reference, $"Enum member value should not be decimal: {ValueLiteral.Value}");
        }
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, $"enum member: {Label()}", string.Empty, SymbolKind.EnumMember);
    }

    private string Label()
    {
        var parentEnum = Parent as EnumDefinition;
        return ValueLiteral != null
            ? $"{parentEnum?.Name}.{Name} = {ValueLiteral.Value}"
            : $"{parentEnum?.Name}.{Name}";
    }

    public override string ToString() => Label();
}
