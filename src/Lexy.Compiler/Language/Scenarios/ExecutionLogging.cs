using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Scenarios;

public class ExecutionLogging : ParsableNode
{
    private readonly List<ExecutionLog> entries = new();

    public IReadOnlyList<ExecutionLog> Entries => entries;

    public ExecutionLogging(Scenario parent, SourceReference reference) :
        base(new NodeReference(parent), reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        return ParseEntry(context);
    }

    private IParsableNode ParseEntry(IParseLineContext context)
    {
        var entry = ExecutionLog.ParseLog(new NodeReference(this), context);
        if (entry == null) return this;
        entries.Add(entry);
        return entry;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return entries;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol() => null;
}
