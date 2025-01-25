using Lexy.Compiler.Language.VariableTypes;

namespace Lexy.Compiler.Parser.Tokens;

public interface ILiteralToken : IToken
{
    object TypedValue { get; }

    string Value { get; }

    VariableType DeriveType(IValidationContext context);
}