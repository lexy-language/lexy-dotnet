using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Parser.Tokens;

public interface ILiteralToken : IToken
{
    object TypedValue { get; }

    string Value { get; }

    Type DeriveType(IValidationContext context);
}