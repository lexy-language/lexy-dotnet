using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ExpectErrors : ErrorsNode<ExpectErrors>
{
    public ExpectErrors(SourceReference reference) : base(reference)
    {
    }
}