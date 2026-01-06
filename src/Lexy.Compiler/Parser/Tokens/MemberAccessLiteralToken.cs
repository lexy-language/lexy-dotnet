using Lexy.Compiler.Language;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public class MemberAccessLiteralToken : Token, ILiteralToken
{
    public string Parent => Parts.Length >= 1 ? Parts[0] : null;
    public string Member => Parts.Length >= 2 ? Parts[1] : null;

    public string[] Parts { get; }

    public override string Value { get; }

    public object TypedValue => Parts;

    public MemberAccessLiteralToken(string value, TokenCharacter character) : base(character)
    {
        Value = Assert.NotNull(value, nameof(value));
        Parts = value.Split(TokenValues.MemberAccess);
    }

    public VariableType DeriveType(IValidationContext context)
    {
        var variableReference = new IdentifierPath(Parts);
        var variableType = context.VariableContext.GetVariableType(variableReference, context);
        if (variableType != null) return variableType;

        if (Parts.Length != 2) return null;

        var componentType = context.ComponentNodes.GetType(Parent);
        return componentType is IObjectType objectType
            ? objectType.MemberType(Member, context.ComponentNodes)
            : null;
    }

    public override string ToString()
    {
        return Value;
    }
}