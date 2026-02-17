using System;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser.Documents;

public interface ISourceCodeDocument : IDisposable
{
    IFile File { get; }

    bool HasMoreLines();
    Line NextLine();
}
