
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser.Tokens
{
    public interface ILiteralToken : IToken
    {
        string Value { get; }

        VariableType DeriveType(IValidationContext context);
    }
}