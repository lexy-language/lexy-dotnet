namespace Lexy.Compiler.Language.Scenarios;

public class ExpectErrors : ErrorsNode<ExpectErrors>
{
    public ExpectErrors(Scenario parent, SourceReference reference) :
        base(new NodeReference(parent), reference)
    {
    }
}
