namespace Lexy.Compiler.Language.Scenarios;

public class ExpectComponentErrors : ErrorsNode<ExpectComponentErrors>
{
    public ExpectComponentErrors(Scenario scenario, SourceReference reference) :
        base(new NodeReference(scenario), reference)
    {
    }
}
