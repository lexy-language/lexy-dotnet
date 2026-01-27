using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.Scenarios;

public class FunctionName : Node
{
    public string Value { get; private set; }

    public FunctionName(SourceReference reference) : base(reference)
    {
    }

    public void Parse(IParseLineContext context)
    {
        var line = context.Line;
        Value = line.Tokens.TokenValue(1);
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

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(Value);
    }

    public override Symbol GetSymbol() => null;
}
