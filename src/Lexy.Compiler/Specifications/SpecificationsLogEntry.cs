using System.Collections.Generic;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Specifications;

public class SpecificationsLogEntry
{
    public IComponentNode Node { get; }
    public SourceReference Reference { get; }
    public bool IsError { get; }
    public string Message { get; }
    public IEnumerable<string> Errors { get; }
    public IEnumerable<ExecutionLogEntry> ExecutionLogging { get; }

    public SpecificationsLogEntry(SourceReference reference, IComponentNode node, bool isError,
        string message, IEnumerable<string> errors = null, IEnumerable<ExecutionLogEntry> executionLogging = null)
    {
        Reference = reference;
        Node = node;
        IsError = isError;
        Message = message;
        Errors = errors;
        ExecutionLogging = executionLogging;
    }

    public override string ToString()
    {
        return Errors == null
            ? Message
            : Message + '\n' + Errors.Format(0);
    }
}