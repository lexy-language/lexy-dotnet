using Lexy.Compiler.Language.Expressions;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Scenarios;

public sealed class IdentifierPathParseResult : ParseResult<IdentifierPath>
{
    private IdentifierPathParseResult(IdentifierPath result) : base(result)
    {
    }

    private IdentifierPathParseResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static IdentifierPathParseResult Success(IdentifierPath result)
    {
        return new IdentifierPathParseResult(result);
    }

    public static IdentifierPathParseResult Failed(string errorMessage)
    {
        return new IdentifierPathParseResult(false, errorMessage);
    }
}
