using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

namespace Lexy.Compiler.Specifications;

public class SpecificationsLogEntry
{
    private string Message { get; }
    private IEnumerable<string> Errors { get; }

    public bool IsError => Errors == null || Errors.Any();

    public SpecificationsLogEntry(string message, IEnumerable<string> errors = null)
    {
        Message = message;
        Errors = errors;
    }

    public override string ToString()
    {
        return Errors == null
            ? Message
            : Message + '\n' + Errors.Format(0);
    }
}
