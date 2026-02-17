using System.Linq;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public class IncompleteMemberAccessToken : Token, ILiteralToken
{
    public string[] Parts { get; }

    public override string Value { get; }

    public object TypedValue => Parts;

    public IncompleteMemberAccessToken(string value, TokenCharacter character) : base(character)
    {
        Value = Assert.NotNull(value, nameof(value));
        Parts = value.Split(TokenValues.MemberAccess);
    }

    public Type DeriveType(IValidationContext context) => null;

    public override string ToString() => "unknown: " + Value;
}
