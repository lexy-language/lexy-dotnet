using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Compiler;

public interface ICompilerContext
{
    ILogger<CompilerContext> Logger { get; }
}