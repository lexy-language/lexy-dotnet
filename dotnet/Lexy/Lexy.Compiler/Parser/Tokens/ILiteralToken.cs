
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser.Tokens
{
    public interface ILiteralToken : IToken
    {
        string Value { get; }

        VariableType DeriveType(IValidationContext context);
    }
}