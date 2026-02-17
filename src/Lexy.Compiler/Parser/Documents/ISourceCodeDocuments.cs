using System;
using System.Collections.Generic;

namespace Lexy.Compiler.Parser.Documents;

public interface ISourceCodeDocuments : IDisposable
{
    IEnumerable<ISourceCodeDocument> Documents { get; }
}