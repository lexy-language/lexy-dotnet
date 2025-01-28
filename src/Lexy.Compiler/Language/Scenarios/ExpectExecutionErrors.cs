using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ExpectExecutionErrors : ErrorsNode<ExpectExecutionErrors>
{
    public ExpectExecutionErrors(SourceReference reference) : base(reference)
    {
    }
}