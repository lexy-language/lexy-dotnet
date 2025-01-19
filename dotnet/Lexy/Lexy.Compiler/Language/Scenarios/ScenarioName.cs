using System.Collections.Generic;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Scenarios;

public class ScenarioName : Node
{
    public string Value { get; }

    public ScenarioName(string name, SourceReference reference) : base(reference)
    {
        Value = name;
    }

    public override string ToString()
    {
        return Value;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }
}