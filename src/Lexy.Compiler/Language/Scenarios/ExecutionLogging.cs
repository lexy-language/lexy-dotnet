using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Language.Scenarios;

public class ExecutionLogging : ParsableNode
{
    private readonly List<ExecutionLog> entries = new();

    public IReadOnlyList<ExecutionLog> Entries => entries;

    public ExecutionLogging(SourceReference reference) : base(reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        return ParseEntry(context);
    }

    private IParsableNode ParseEntry(IParseLineContext context)
    {
        var entry = ExecutionLog.ParseLog(context);
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
