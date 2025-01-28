using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ExpectRootErrors : ErrorsNode<ExpectRootErrors>
{
    public ExpectRootErrors(SourceReference reference) : base(reference)
    {
    }
}