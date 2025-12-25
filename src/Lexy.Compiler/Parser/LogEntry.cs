using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

internal class LogEntry
{
    public INode Node { get; }
    public SourceReference Reference { get; }
    public string SortIndex { get; }
    public bool IsError { get; }
    public string Message { get; }

    public LogEntry(SourceReference reference, INode node, bool isError, string message)
    {
        Node = node;
        IsError = isError;
        Message = message;
        SortIndex = reference.SortIndex;
        Reference = reference;
    }

    public override string ToString()
    {
        return Node != null
            ? $"({Node?.GetType().Name}) {Message}"
            : Message;
    }
}