using Lexy.Compiler.Language.Expressions;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Scenarios;

public sealed class VariablePathParseResult : ParseResult<IdentifierPath>
{
    private VariablePathParseResult(IdentifierPath result) : base(result)
    {
    }

    private VariablePathParseResult(bool success, string errorMessage) : base(success, errorMessage)
    {
    }

    public static VariablePathParseResult Success(IdentifierPath result)
    {
        return new VariablePathParseResult(result);
    }

    public static VariablePathParseResult Failed(string errorMessage)
    {
        return new VariablePathParseResult(false, errorMessage);
    }
}