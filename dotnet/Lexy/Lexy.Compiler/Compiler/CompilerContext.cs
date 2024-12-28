using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Compiler;

public class CompilerContext : ICompilerContext
{
    public CompilerContext(ILogger<CompilerContext> logger)
    {
        Logger = logger;
    }

    public ILogger<CompilerContext> Logger { get; }
}