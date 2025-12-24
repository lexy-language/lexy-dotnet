using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ExpectComponentErrors : ErrorsNode<ExpectComponentErrors>
{
    public ExpectComponentErrors(SourceReference reference) : base(reference)
    {
    }
}