namespace Lexy.Compiler.Language.Scenarios;

public class ExpectExecutionErrors : ErrorsNode<ExpectExecutionErrors>
{
    public ExpectExecutionErrors(Scenario scenario, SourceReference reference) :
        base(new NodeReference(scenario), reference)
    {
    }
}
