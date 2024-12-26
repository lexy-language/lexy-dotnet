using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public class CompilerContext : ICompilerContext
    {
        public ILogger<CompilerContext> Logger { get; }

        public CompilerContext(ILogger<CompilerContext> logger)
        {
            this.Logger = logger;
        }
    }
}