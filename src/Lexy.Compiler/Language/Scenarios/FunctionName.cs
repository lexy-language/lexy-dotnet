using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class FunctionName : Node
{
    public string Value { get; }

    private FunctionName(string value, Scenario parent, SourceReference reference) : base(new NodeReference(parent), reference)
    {
        Value = value;
    }

    public static FunctionName Parse(IParseLineContext context, Scenario parent, SourceReference reference)
    {
        var line = context.Line;
        var name = line.Tokens.TokenValue(1);

        return new FunctionName(name, parent, reference);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(Value);
    }

    public override Symbol GetSymbol() => null;

    public override string ToString() => Value;
}
